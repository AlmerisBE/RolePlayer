namespace RolePlayer.Core.GameEngine.Contracts;

using RolePlayer.Core.GameEngine.Models;
using System;

public interface IGameSessionService {
    event Action? SessionStateChanged;

    SessionState CurrentState { get; }
    GameSessionConfig? CurrentConfig { get; }

    bool StartSession(GameSessionConfig config);
    void StopSession();
}