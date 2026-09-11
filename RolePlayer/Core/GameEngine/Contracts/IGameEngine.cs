namespace RolePlayer.Core.GameEngine.Contracts;

using RolePlayer.Core.GameEngine.Models;
using System;
using System.Collections.Generic;

public interface IGameEngine {
    string EngineType { get; }
    bool IsRunning { get; }
    IReadOnlyList<string> Participants { get; }

    event Action<string>? BroadcastRequested;
    event Action? GameFinished;
    event Action? ParticipantsChanged;

    void Initialize(GameSessionConfig config);
    void Start();
    void Stop();
    void ProcessEvent(GameEvent gameEvent);
}