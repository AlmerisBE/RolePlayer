namespace RolePlayer.Core.GameEngine.Services.Handlers;

using RolePlayer.Core.GameEngine.Contracts;
using RolePlayer.Core.GameEngine.Models;
using System;
using System.Collections.Generic;
using System.Linq;

public class EvaluateMinMaxHandler : IGameActionHandler {
    public string ActionType => "EvaluateMinMax";

    public void Execute(GameActionConfig action, GameSessionContext context, IGameActionExecutionService executionService) {
        string prefix = action.Parameters.TryGetValue("Prefix", out var pfx) ? pfx : "score_";
        string notEnoughMsg = action.Parameters.TryGetValue("NotEnoughParticipants", out var nrm) ? nrm : "[System] Not enough participants for evaluation.";
        int minParticipants = int.TryParse(action.Parameters.TryGetValue("MinParticipants", out var mp) ? mp : "1", out int parsed) ? parsed : 1;

        var scores = new List<(string Name, int Score)>();

        List<string>? allowedPlayers = null;
        if (action.Parameters.TryGetValue("FilterVar", out var filterVar) && context.Variables.TryGetValue(filterVar, out var filterObj) && filterObj is List<string> ep) {
            allowedPlayers = ep;
        }

        foreach (var kvp in context.Variables.Where(k => k.Key.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))) {
            if (int.TryParse(kvp.Value?.ToString(), out int s)) {
                string playerName = kvp.Key.Substring(prefix.Length);
                if (allowedPlayers == null || allowedPlayers.Contains(playerName, StringComparer.OrdinalIgnoreCase)) {
                    scores.Add((playerName, s));
                }
            }
        }

        if (scores.Count < minParticipants) {
            executionService.RequestBroadcast(executionService.FormatString(notEnoughMsg, context));
            context.Variables["tod_resolving"] = "done";
            return;
        }

        int maxScore = scores.Max(s => s.Score);
        int minScore = scores.Min(s => s.Score);

        context.Variables["max_score"] = maxScore;
        context.Variables["min_score"] = minScore;
        context.Variables["max_players"] = scores.Where(s => s.Score == maxScore).Select(s => s.Name).ToList();
        context.Variables["min_players"] = scores.Where(s => s.Score == minScore).Select(s => s.Name).ToList();
    }
}