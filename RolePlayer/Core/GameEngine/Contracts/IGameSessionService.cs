namespace RolePlayer.Core.GameEngine.Contracts;

using RolePlayer.Core.GameEngine.Models;
using System;
using System.Collections.Generic;

public interface IGameSessionService {
    event Action? SessionStateChanged;

    SessionState CurrentState { get; }
    GameSessionConfig? CurrentConfig { get; }
    IReadOnlyList<string> Participants { get; }
    IReadOnlyDictionary<string, object> SessionVariables { get; }

    bool StartSession(GameSessionConfig config);
    void StopSession();

    string CurrentStageName { get; }
    string CurrentStageDescription { get; }
    bool AllowChatRegistration { get; set; }
    void AddParticipant(string name);
    void RemoveParticipant(string name);
    void AdvanceStage();
    void SetSessionVariable(string key, object value);
    IReadOnlyList<TimeSpan> RemainingTimers { get; }
}