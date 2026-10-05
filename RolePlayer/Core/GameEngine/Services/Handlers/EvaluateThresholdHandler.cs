namespace RolePlayer.Core.GameEngine.Services.Handlers;

using RolePlayer.Core.GameEngine.Contracts;
using RolePlayer.Core.GameEngine.Models;
using System;
using System.Collections.Generic;
using System.Linq;

public class EvaluateThresholdHandler : IGameActionHandler {
    public string ActionType => "EvaluateThreshold";

    public void Execute(GameActionConfig action, GameSessionContext context, IGameActionExecutionService executionService) {
        string prefix = action.Parameters.TryGetValue("Prefix", out var pfx) ? pfx : "score_";
        string op = action.Parameters.TryGetValue("Operator", out var o) ? o : "<=";
        string rawThreshold = action.Parameters.TryGetValue("Threshold", out var r) ? r : "0";
        string passMsg = action.Parameters.TryGetValue("PassMessage", out var pm) ? pm : "{Players} matched!";
        string failMsg = action.Parameters.TryGetValue("FailMessage", out var fm) ? fm : "No one matched!";
        string notEnoughMsg = action.Parameters.TryGetValue("NotEnoughParticipants", out var nrm) ? nrm : failMsg;

        int threshold = int.TryParse(executionService.FormatString(rawThreshold, context), out int t) ? t : 0;

        var allScores = context.Variables.Where(k => k.Key.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)).ToList();

        if (allScores.Count == 0) {
            executionService.RequestBroadcast(executionService.FormatString(notEnoughMsg, context));
            return;
        }

        var matchingPlayers = new List<string>();
        foreach (var kvp in allScores) {
            if (int.TryParse(kvp.Value?.ToString(), out int s)) {
                bool matches = op switch {
                    "<=" => s <= threshold,
                    "<" => s < threshold,
                    ">=" => s >= threshold,
                    ">" => s > threshold,
                    "==" => s == threshold,
                    _ => false
                };
                if (matches) matchingPlayers.Add(kvp.Key.Substring(prefix.Length));
            }
        }

        if (matchingPlayers.Count > 0) {
            string playersStr = string.Join(", ", matchingPlayers);
            string finalMsg = passMsg.Replace("{Players}", playersStr, StringComparison.OrdinalIgnoreCase);
            executionService.RequestBroadcast(finalMsg);
        }
        else {
            executionService.RequestBroadcast(failMsg);
        }
    }
}