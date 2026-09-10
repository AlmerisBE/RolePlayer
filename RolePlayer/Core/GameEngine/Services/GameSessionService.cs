namespace RolePlayer.Core.GameEngine.Services;

using RolePlayer.Core.GameEngine.Contracts;
using RolePlayer.Core.GameEngine.Models;
using RolePlayer.Core.Logging.Contracts;
using System;
using System.Linq;

public class GameSessionService : IGameSessionService {
    private ILoggerService logger;
    private IGameEngineFactory engineFactory;
    private IGameEngine? activeEngine;

    public event Action? SessionStateChanged;

    public SessionState CurrentState { get; private set; } = SessionState.Inactive;
    public GameSessionConfig? CurrentConfig { get; private set; }

    public GameSessionService(ILoggerService logger, IGameEngineFactory engineFactory) {
        this.logger = logger;
        this.engineFactory = engineFactory;
    }

    public bool StartSession(GameSessionConfig config) {
        if (this.CurrentState != SessionState.Inactive) {
            this.logger.Warning("Attempted to start a game session while another is already active.");
            return false;
        }

        if (config == null || config.Game == null || string.IsNullOrWhiteSpace(config.Game.EngineType)) {
            this.logger.Warning("Attempted to start a game session with a missing or invalid GameDefinition.");
            return false;
        }

        if (!config.ListeningChannels.Any()) {
            this.logger.Warning("Attempted to start a game session without defining any listening channels.");
            return false;
        }

        this.activeEngine = this.engineFactory.CreateEngine(config.Game.EngineType);
        if (this.activeEngine == null) {
            this.logger.Error($"Failed to resolve game engine for type: {config.Game.EngineType}");
            return false;
        }

        this.CurrentConfig = config;
        this.CurrentState = SessionState.WaitingForPlayers;

        this.activeEngine.GameFinished += this.OnGameFinished;
        this.activeEngine.Initialize(config);
        this.activeEngine.Start();

        this.logger.Info($"Started new game session: {config.Game.Name} using {config.Game.EngineType}.");
        this.SessionStateChanged?.Invoke();

        return true;
    }

    public void StopSession() {
        if (this.CurrentState == SessionState.Inactive) return;

        if (this.activeEngine != null) {
            this.activeEngine.GameFinished -= this.OnGameFinished;
            this.activeEngine.Stop();
            this.activeEngine = null;
        }

        this.CurrentState = SessionState.Inactive;
        this.CurrentConfig = null;

        this.logger.Info("Game session has been stopped.");
        this.SessionStateChanged?.Invoke();
    }

    private void OnGameFinished() {
        this.CurrentState = SessionState.Finished;
        this.SessionStateChanged?.Invoke();
    }
}