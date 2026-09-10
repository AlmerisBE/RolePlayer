namespace RolePlayer.Core.GameEngine.Services;

using RolePlayer.Core.GameEngine.Contracts;
using RolePlayer.Core.GameEngine.Models;
using RolePlayer.Core.Logging.Contracts;
using System;
using System.Linq;

public class GameSessionService : IGameSessionService {
    private ILoggerService logger;

    public event Action? SessionStateChanged;

    public SessionState CurrentState { get; private set; } = SessionState.Inactive;
    public GameSessionConfig? CurrentConfig { get; private set; }

    public GameSessionService(ILoggerService logger) {
        this.logger = logger;
    }

    public bool StartSession(GameSessionConfig config) {
        if (this.CurrentState != SessionState.Inactive) {
            this.logger.Warning("Attempted to start a game session while another is already active.");
            return false;
        }

        if (config == null || config.Game == null) {
            this.logger.Warning("Attempted to start a game session with a missing GameDefinition.");
            return false;
        }

        if (!config.ListeningChannels.Any()) {
            this.logger.Warning("Attempted to start a game session without defining any listening channels.");
            return false;
        }

        this.CurrentConfig = config;
        this.CurrentState = SessionState.WaitingForPlayers;

        this.logger.Info($"Started new game session: {config.Game.Name}. Listening on {config.ListeningChannels.Count} channels.");
        this.SessionStateChanged?.Invoke();

        return true;
    }

    public void StopSession() {
        if (this.CurrentState == SessionState.Inactive) return;

        this.CurrentState = SessionState.Inactive;
        this.CurrentConfig = null;

        this.logger.Info("Game session has been stopped.");
        this.SessionStateChanged?.Invoke();
    }
}