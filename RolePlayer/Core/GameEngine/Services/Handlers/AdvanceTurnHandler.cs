namespace RolePlayer.Core.GameEngine.Services.Handlers;

using RolePlayer.Core.GameEngine.Contracts;
using RolePlayer.Core.GameEngine.Models;
using System;

public class AdvanceTurnHandler : IGameActionHandler {
    public string ActionType => "AdvanceTurn";

    public void Execute(GameActionConfig action, GameSessionContext context, IGameActionExecutionService executionService) {
        if (!action.Parameters.TryGetValue("TargetVar", out var targetVar) || string.IsNullOrWhiteSpace(targetVar)) return;
        if (context.Participants.Count == 0) return;

        string currentPlayer = context.Variables.TryGetValue(targetVar, out var val) ? val?.ToString() ?? string.Empty : string.Empty;
        int currentIndex = context.Participants.FindIndex(p => p.Equals(currentPlayer, StringComparison.OrdinalIgnoreCase));

        int nextIndex = currentIndex < 0 ? 0 : (currentIndex + 1) % context.Participants.Count;
        context.Variables[targetVar] = context.Participants[nextIndex];
    }
}