namespace RolePlayer.Core.GameEngine.Contracts;

using RolePlayer.Core.GameEngine.Models;
using System;
using System.Collections.Generic;

public interface IGameEngine {
    string EngineType { get; }
    bool IsRunning { get; }
    IReadOnlyList<string> Participants { get; }
    IReadOnlyDictionary<string, object> Variables { get; }
    string CurrentStageName { get; }
    bool AllowChatRegistration { get; set; }

    event Action<string>? BroadcastRequested;
    event Action? GameFinished;
    event Action? ParticipantsChanged;
    event Action? StageChanged;

    void Initialize(GameSessionConfig config);
    void Start();
    void Stop();
    void ProcessEvent(GameEvent gameEvent);

    void AddParticipant(string name);
    void RemoveParticipant(string name);
    void AdvanceStage();
}