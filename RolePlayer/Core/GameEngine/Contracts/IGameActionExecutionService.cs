namespace RolePlayer.Core.GameEngine.Contracts;

using RolePlayer.Core.GameEngine.Models;
using System;
using System.Collections.Generic;

public interface IGameActionExecutionService {
    event Action<string>? BroadcastRequested;
    event Action? StageAdvanceRequested;
    event Action? GameStopRequested;

    void ExecuteAll(IEnumerable<GameActionConfig> actions, GameSessionContext context);
    void Execute(GameActionConfig action, GameSessionContext context);

    // Callbacks for handlers
    void RequestBroadcast(string message);
    void RequestStageAdvance();
    void RequestGameStop();
    string FormatString(string input, GameSessionContext context);
}