namespace RolePlayer.Core.GameEngine.Services;

using RolePlayer.Core.GameEngine.Contracts;
using RolePlayer.Core.GameEngine.Models;
using RolePlayer.Core.Logging.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;

public class GameSessionService : IGameSessionService {
    private ILoggerService logger;
    private IGameEngineFactory engineFactory;
    private IEnumerable<IGameEventWatcher> eventWatchers;
    private IGameEngine? activeEngine;
    private IChatBroadcaster chatBroadcaster;

    public event Action? SessionStateChanged;

    public SessionState CurrentState { get; private set; } = SessionState.Inactive;
    public GameSessionConfig? CurrentConfig { get; private set; }

    public GameSessionService(
        ILoggerService logger,
        IGameEngineFactory engineFactory,
        IEnumerable<IGameEventWatcher> eventWatchers,
        IChatBroadcaster chatBroadcaster) {

        this.logger = logger;
        this.engineFactory = engineFactory;
        this.eventWatchers = eventWatchers;
        this.chatBroadcaster = chatBroadcaster;
    }

    public IReadOnlyList<string> Participants => this.activeEngine?.Participants ?? new List<string>();
    public IReadOnlyDictionary<string, object> SessionVariables => this.activeEngine?.Variables ?? new Dictionary<string, object>();

    public string CurrentStageName => this.activeEngine?.CurrentStageName ?? string.Empty;

    public bool AllowChatRegistration {
        get => this.activeEngine?.AllowChatRegistration ?? false;
        set {
            if (this.activeEngine != null) {
                this.activeEngine.AllowChatRegistration = value;
                foreach (var watcher in this.eventWatchers) {
                    watcher.RestrictToParticipants = !value;
                }
            }
        }
    }

    public bool StartSession(GameSessionConfig config) {
        if (this.CurrentState != SessionState.Inactive) {
            this.logger.Warning("Attempted to start a game session while another is already active.");
            return false;
        }

        if (config == null || config.Game == null) {
            this.logger.Warning("Attempted to start a game session with a missing or invalid GameDefinition.");
            return false;
        }

        if (!config.ListeningChannels.Any()) {
            this.logger.Warning("Attempted to start a game session without defining any listening channels.");
            return false;
        }

        this.activeEngine = this.engineFactory.CreateEngine("StateMachineEngine");
        if (this.activeEngine == null) {
            this.logger.Error("Failed to resolve universal StateMachineEngine.");
            return false;
        }

        this.CurrentConfig = config;
        this.CurrentState = SessionState.WaitingForPlayers;

        this.activeEngine.GameFinished += this.OnGameFinished;
        this.activeEngine.BroadcastRequested += this.OnBroadcastRequested;
        this.activeEngine.ParticipantsChanged += this.OnParticipantsChanged;
        this.activeEngine.Initialize(config);

        foreach (var watcher in this.eventWatchers) {
            watcher.ClearParticipants();
            watcher.EventFired += this.OnGameEventFired;
            watcher.Start();
        }

        this.activeEngine.Start();

        // Le moteur est démarré, il a lu sa définition JSON. On synchronise l'état initial.
        foreach (var watcher in this.eventWatchers) {
            watcher.RestrictToParticipants = !this.activeEngine.AllowChatRegistration;
        }

        this.logger.Info($"Started new game session: {config.Game.Name}.");
        this.SessionStateChanged?.Invoke();

        return true;
    }

    public void StopSession() {
        if (this.CurrentState == SessionState.Inactive) return;

        foreach (var watcher in this.eventWatchers) {
            watcher.EventFired -= this.OnGameEventFired;
            watcher.ClearParticipants();
            watcher.Stop();
        }

        if (this.activeEngine != null) {
            this.activeEngine.GameFinished -= this.OnGameFinished;
            this.activeEngine.BroadcastRequested -= this.OnBroadcastRequested;
            this.activeEngine.ParticipantsChanged -= this.OnParticipantsChanged;
            this.activeEngine.Stop();
            this.activeEngine = null;
        }

        this.CurrentState = SessionState.Inactive;
        this.CurrentConfig = null;

        this.logger.Info("Game session has been stopped.");
        this.SessionStateChanged?.Invoke();
    }

    private void OnBroadcastRequested(string message) {
        if (this.CurrentConfig == null || !this.CurrentConfig.ListeningChannels.Any()) return;

        var targetChannel = this.CurrentConfig.ListeningChannels.First();
        this.chatBroadcaster.Broadcast(message, targetChannel);
    }

    private void OnGameEventFired(GameEvent gameEvent) {
        this.activeEngine?.ProcessEvent(gameEvent);
    }

    private void OnGameFinished() {
        this.StopSession();
        this.CurrentState = SessionState.Finished;
        this.SessionStateChanged?.Invoke();
    }

    private void OnParticipantsChanged() {
        foreach (var watcher in this.eventWatchers) {
            watcher.SetParticipants(this.Participants);
        }

        this.SessionStateChanged?.Invoke();
    }

    public void AddParticipant(string name) {
        if (this.activeEngine != null) this.activeEngine.AddParticipant(name);
    }

    public void RemoveParticipant(string name) {
        if (this.activeEngine != null) this.activeEngine.RemoveParticipant(name);
    }

    public void AdvanceStage() {
        if (this.activeEngine != null) this.activeEngine.AdvanceStage();
    }
}