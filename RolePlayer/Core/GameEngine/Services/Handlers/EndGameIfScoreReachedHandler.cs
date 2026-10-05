namespace RolePlayer.Core.GameEngine.Services.Handlers;

using RolePlayer.Core.GameEngine.Contracts;
using RolePlayer.Core.GameEngine.Models;
using System;

public class EndGameIfScoreReachedHandler : IGameActionHandler {
    public string ActionType => "EndGameIfScoreReached";

    public void Execute(GameActionConfig action, GameSessionContext context, IGameActionExecutionService executionService) {
        if (!action.Parameters.TryGetValue("Prefix", out var prefix)) prefix = "score_";
        if (!action.Parameters.TryGetValue("TargetScore", out var rawTarget)) return;

        string targetStr = executionService.FormatString(rawTarget, context);
        if (!int.TryParse(targetStr, out int targetScore)) return;

        foreach (var kvp in context.Variables) {
            if (kvp.Key.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)) {
                int score = int.TryParse(kvp.Value?.ToString(), out int s) ? s : 0;
                if (score >= targetScore) {
                    string winner = kvp.Key.Substring(prefix.Length);
                    executionService.RequestBroadcast($"[Game Over] {winner} reached {targetScore} points and wins the game!");
                    executionService.RequestGameStop();
                    return;
                }
            }
        }
    }
}