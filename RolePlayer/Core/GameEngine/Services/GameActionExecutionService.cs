namespace RolePlayer.Core.GameEngine.Services;

using RolePlayer.Core.GameEngine.Contracts;
using RolePlayer.Core.GameEngine.Models;
using System;
using System.Collections.Generic;
using System.Linq;

public class GameActionExecutionService : IGameActionExecutionService {
    public event Action<string>? BroadcastRequested;
    public event Action? StageAdvanceRequested;
    public event Action? GameStopRequested;

    private Dictionary<string, IGameActionHandler> handlers;

    public GameActionExecutionService(IEnumerable<IGameActionHandler> actionHandlers) {
        this.handlers = actionHandlers.ToDictionary(h => h.ActionType, StringComparer.OrdinalIgnoreCase);
    }

    public void ExecuteAll(IEnumerable<GameActionConfig> actions, GameSessionContext context) {
        if (actions == null) return;

        foreach (var action in actions) {
            this.Execute(action, context);
        }
    }

    public void Execute(GameActionConfig action, GameSessionContext context) {
        if (action == null || string.IsNullOrWhiteSpace(action.ActionType)) return;

        if (this.handlers.TryGetValue(action.ActionType, out var handler)) {
            handler.Execute(action, context, this);
        }
    }

    public void RequestBroadcast(string message) => this.BroadcastRequested?.Invoke(message);
    public void RequestStageAdvance() => this.StageAdvanceRequested?.Invoke();
    public void RequestGameStop() => this.GameStopRequested?.Invoke();

    public string FormatString(string input, GameSessionContext context) {
        if (string.IsNullOrWhiteSpace(input)) return input;

        string result = input;

        if (context.CurrentEvent != null) {
            result = result.Replace("{Event.Sender}", context.CurrentEvent.Sender, StringComparison.OrdinalIgnoreCase);

            if (context.CurrentEvent is DiceRollGameEvent dice) {
                result = result.Replace("{Event.Roll}", dice.Roll.ToString(), StringComparison.OrdinalIgnoreCase);
                result = result.Replace("{Event.MaxRoll}", dice.MaxRoll.ToString(), StringComparison.OrdinalIgnoreCase);
            }
        }

        result = result.Replace("{Participants.Count}", context.Participants.Count.ToString(), StringComparison.OrdinalIgnoreCase);

        foreach (var kvp in context.Variables) {
            if (kvp.Value is not List<string>) {
                result = result.Replace($"{{Var.{kvp.Key}}}", kvp.Value?.ToString() ?? string.Empty, StringComparison.OrdinalIgnoreCase);
            }
        }

        return result;
    }
}