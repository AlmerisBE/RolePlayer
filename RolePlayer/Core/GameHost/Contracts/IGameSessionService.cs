namespace RolePlayer.Core.GameHost.Contracts;

using RolePlayer.Core.GameHost.Models;
using System;

public interface IGameSessionService {
    event Action? SessionStateChanged;

    SessionState CurrentState { get; }
    GameSessionConfig? CurrentConfig { get; }

    bool StartSession(GameSessionConfig config);
    void StopSession();
}