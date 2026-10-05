namespace RolePlayer.Core.GameEngine.Services.Handlers;

using RolePlayer.Core.GameEngine.Contracts;
using RolePlayer.Core.GameEngine.Models;
using System;
using System.Collections.Generic;
using System.Linq;

public class BroadcastScoresHandler : IGameActionHandler {
    public string ActionType => "BroadcastScores";

    public void Execute(GameActionConfig action, GameSessionContext context, IGameActionExecutionService executionService) {
        if (!action.Parameters.TryGetValue("Prefix", out var prefix)) prefix = "score_";

        var scores = new List<(string Name, int Score)>();
        foreach (var kvp in context.Variables) {
            if (kvp.Key.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)) {
                string playerName = kvp.Key.Substring(prefix.Length);
                int score = int.TryParse(kvp.Value?.ToString(), out int s) ? s : 0;
                scores.Add((playerName, score));
            }
        }

        if (scores.Count == 0) {
            executionService.RequestBroadcast("[Scores] No scores recorded yet.");
            return;
        }

        scores = scores.OrderByDescending(s => s.Score).ToList();

        var scoreStrings = scores.Select((s, index) => $"{index + 1}. {s.Name} ({s.Score} pts)");
        string leaderboard = $"[Leaderboard] {string.Join(" | ", scoreStrings)}";

        executionService.RequestBroadcast(leaderboard);
    }
}