namespace RolePlayer.Core.GameEngine.Contracts;

using RolePlayer.Core.GameEngine.Models;
using System;
using System.Collections.Generic;

public interface IGameSessionService {
    event Action? SessionStateChanged;

    SessionState CurrentState { get; }
    GameSessionConfig? CurrentConfig { get; }
    IReadOnlyList<string> Participants { get; }

    bool StartSession(GameSessionConfig config);
    void StopSession();
}