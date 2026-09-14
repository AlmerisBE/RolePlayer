namespace RolePlayer.UI.GameDashboard.Presenters;

using Dalamud.Plugin.Services;
using RolePlayer.Core.Emotes.Contracts;
using RolePlayer.Core.Emotes.Models;
using RolePlayer.Core.GameEngine.Contracts;
using RolePlayer.Core.GameEngine.Models;
using RolePlayer.UI.GameDashboard.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;

public class GameDashboardPresenter : IGameDashboardPresenter {
    private IGameLibraryService libraryService;
    private IGameSessionService sessionService;
    private ITargetManager targetManager;
    private IEmoteCache emoteCache;

    public IReadOnlyList<GameDefinition> AvailableGames => this.libraryService.GetAvailableGames().ToList();
    public IReadOnlyList<EnrichedEmote> EmotesCache => this.emoteCache.GetCachedEmotes();
    public GameDefinition? SelectedGame { get; private set; }
    public GameChatChannel SelectedBroadcastChannel { get; private set; } = GameChatChannel.Say;
    public HashSet<GameChatChannel> SelectedChannels { get; private set; } = new() { GameChatChannel.Say };
    public SessionState CurrentState => this.sessionService.CurrentState;
    public IReadOnlyList<string> Participants => this.sessionService.Participants;
    public IReadOnlyDictionary<string, object> ActiveVariables {
        get {
            if (this.CurrentState != SessionState.Inactive) return this.sessionService.SessionVariables;
            return this.SelectedGame?.InitialVariables ?? new Dictionary<string, object>();
        }
    }
    public IReadOnlyList<TimeSpan> RemainingTimers => this.sessionService.RemainingTimers;
    public string CurrentStageName => this.sessionService.CurrentStageName;
    public string NextManualStageName => this.sessionService.NextManualStageName;
    public string CurrentStageDescription => this.sessionService.CurrentStageDescription;
    public string CurrentTargetName => this.targetManager.Target?.Name.TextValue ?? string.Empty;
    public string LastErrorKey { get; private set; } = string.Empty;

    public bool AllowChatRegistration {
        get {
            if (this.CurrentState != SessionState.Inactive) return this.sessionService.AllowChatRegistration;
            return this.SelectedGame?.AllowChatRegistration ?? false;
        }
        set {
            if (this.CurrentState != SessionState.Inactive) {
                this.sessionService.AllowChatRegistration = value;
            }
            else if (this.SelectedGame != null) {
                this.SelectedGame.AllowChatRegistration = value;
                this.SaveGameConfig();
            }
        }
    }

    public GameDashboardPresenter(IGameLibraryService libraryService, IGameSessionService sessionService, ITargetManager targetManager, IEmoteCache emoteCache) {
        this.libraryService = libraryService;
        this.sessionService = sessionService;
        this.targetManager = targetManager;
        this.emoteCache = emoteCache;

        this.sessionService.ErrorReported += key => this.LastErrorKey = key;
        this.sessionService.SessionStateChanged += this.DismissError;
    }

    public void SelectGame(GameDefinition? game) {
        if (this.CurrentState != SessionState.Inactive) return;
        this.SelectedGame = game;
    }

    public void ToggleChannel(GameChatChannel channel) {
        if (this.CurrentState != SessionState.Inactive) return;
        if (!this.SelectedChannels.Add(channel)) this.SelectedChannels.Remove(channel);
    }

    public void SetBroadcastChannel(GameChatChannel channel) {
        if (this.CurrentState != SessionState.Inactive) return;

        this.SelectedBroadcastChannel = channel;
        this.SelectedChannels.Add(channel);
    }

    public void StartSession() {
        if (this.SelectedGame == null || !this.SelectedChannels.Any()) return;

        var config = new GameSessionConfig {
            Game = this.SelectedGame,
            BroadcastChannel = this.SelectedBroadcastChannel,
            ListeningChannels = new HashSet<GameChatChannel>(this.SelectedChannels)
        };

        this.sessionService.StartSession(config);
    }

    public void StopSession() {
        this.sessionService.StopSession();
    }

    public void SetVariable(string key, object value) {
        if (this.CurrentState != SessionState.Inactive) {
            this.sessionService.SetSessionVariable(key, value);
        }
        else if (this.SelectedGame != null) {
            this.SelectedGame.InitialVariables[key] = value;
            this.SaveGameConfig();
        }
    }

    public void DismissError() {
        this.LastErrorKey = string.Empty;
    }

    public void AdvanceStage() {
        this.DismissError();
        this.sessionService.AdvanceStage();
    }

    public void AddTarget() {
        var targetName = this.CurrentTargetName;
        if (!string.IsNullOrWhiteSpace(targetName)) this.sessionService.AddParticipant(targetName);
    }

    public void RemoveParticipant(string name) {
        if (!string.IsNullOrWhiteSpace(name)) this.sessionService.RemoveParticipant(name);
    }

    public void SaveGameConfig() {
        if (this.SelectedGame != null) this.libraryService.SaveGame(this.SelectedGame);
    }

    public void Dispose() {
        this.sessionService.StopSession();
    }
}