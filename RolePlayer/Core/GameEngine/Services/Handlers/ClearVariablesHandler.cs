namespace RolePlayer.Core.GameEngine.Services.Handlers;

using RolePlayer.Core.GameEngine.Contracts;
using RolePlayer.Core.GameEngine.Models;
using System;
using System.Linq;

public class ClearVariablesHandler : IGameActionHandler {
    public string ActionType => "ClearVariables";

    public void Execute(GameActionConfig action, GameSessionContext context, IGameActionExecutionService executionService) {
        if (!action.Parameters.TryGetValue("Prefix", out var prefix) || string.IsNullOrEmpty(prefix)) return;

        var keysToRemove = context.Variables.Keys.Where(k => k.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)).ToList();
        foreach (var key in keysToRemove) {
            context.Variables.Remove(key);
        }
    }
}