namespace RolePlayer.Core.GameEngine.Services.Handlers;

using RolePlayer.Core.GameEngine.Contracts;
using RolePlayer.Core.GameEngine.Models;
using System;
using System.Collections.Generic;
using System.Linq;

public class ResolveBlackjackWinnerHandler : IGameActionHandler {
    public string ActionType => "ResolveBlackjackWinner";

    public void Execute(GameActionConfig action, GameSessionContext context, IGameActionExecutionService executionService) {
        if (!action.Parameters.TryGetValue("ScorePrefix", out var prefix)) prefix = "score_";
        if (!action.Parameters.TryGetValue("TargetScore", out var rawTarget)) rawTarget = "21";

        string targetStr = executionService.FormatString(rawTarget, context);
        if (!int.TryParse(targetStr, out int targetScore)) targetScore = 21;

        var validScores = new List<(string Name, int Score)>();

        foreach (var kvp in context.Variables) {
            if (kvp.Key.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)) {
                string playerName = kvp.Key.Substring(prefix.Length);
                int score = int.TryParse(kvp.Value?.ToString(), out int s) ? s : 0;

                if (score <= targetScore) {
                    validScores.Add((playerName, score));
                }
            }
        }

        if (validScores.Count == 0) {
            executionService.RequestBroadcast("[Game Over] Everyone busted! The house wins.");
        }
        else {
            var maxScore = validScores.Max(s => s.Score);
            var winners = validScores.Where(s => s.Score == maxScore).Select(s => s.Name).ToList();

            if (winners.Count > 1) {
                executionService.RequestBroadcast($"[Game Over] It's a tie between {string.Join(" and ", winners)} with {maxScore} points!");
            }
            else {
                executionService.RequestBroadcast($"[Game Over] {winners[0]} wins with {maxScore} points!");
            }
        }

        executionService.RequestGameStop();
    }
}