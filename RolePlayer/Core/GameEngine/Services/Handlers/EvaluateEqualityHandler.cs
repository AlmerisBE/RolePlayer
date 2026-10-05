namespace RolePlayer.Core.GameEngine.Services.Handlers;

using RolePlayer.Core.GameEngine.Contracts;
using RolePlayer.Core.GameEngine.Models;
using System.Collections.Generic;

public class EvaluateEqualityHandler : IGameActionHandler {
    public string ActionType => "EvaluateEquality";

    public void Execute(GameActionConfig action, GameSessionContext context, IGameActionExecutionService executionService) {
        var maxPlayers = context.Variables.TryGetValue("max_players", out var maxObj) && maxObj is List<string> mp ? mp : new List<string>();
        var minPlayers = context.Variables.TryGetValue("min_players", out var minObj) && minObj is List<string> minp ? minp : new List<string>();

        if (maxPlayers.Count == 0 || minPlayers.Count == 0) {
            context.Variables["tod_resolving"] = "done";
            return;
        }

        string maxScore = context.Variables.TryGetValue("max_score", out var mscore) ? mscore.ToString()! : "0";
        string minScore = context.Variables.TryGetValue("min_score", out var minscore) ? minscore.ToString()! : "0";

        string resolvingState = context.Variables.TryGetValue("tod_resolving", out var res) ? res?.ToString() ?? "normal" : "normal";

        if (resolvingState == "normal") {
            if (maxPlayers.Count > 1) {
                context.Variables["tod_resolving"] = "high";
                context.Variables["tod_expected_players"] = maxPlayers;
                context.Variables["tod_pending_low"] = minPlayers;
                executionService.RequestBroadcast($"[Truth or Dare] We have a tie for the HIGHEST score ({maxScore})! {string.Join(", ", maxPlayers)}, you have 60 seconds to /random again!");
            }
            else if (minPlayers.Count > 1) {
                context.Variables["tod_resolving"] = "low";
                context.Variables["tod_expected_players"] = minPlayers;
                context.Variables["tod_final_high"] = maxPlayers[0];
                executionService.RequestBroadcast($"[Truth or Dare] We have a tie for the LOWEST score ({minScore})! {string.Join(", ", minPlayers)}, you have 60 seconds to /random again!");
            }
            else {
                context.Variables["tod_resolving"] = "done";
                executionService.RequestBroadcast($"[Truth or Dare] Highest score: {maxPlayers[0]} ({maxScore}). Lowest score: {minPlayers[0]} ({minScore}).\n{maxPlayers[0]}, you must ask Truth or Dare to {minPlayers[0]}!");
            }
        }
        else if (resolvingState == "high") {
            if (maxPlayers.Count > 1) {
                context.Variables["tod_expected_players"] = maxPlayers;
                executionService.RequestBroadcast($"[Truth or Dare] Still tied for HIGHEST ({maxScore})! {string.Join(", ", maxPlayers)}, please /random again!");
            }
            else {
                context.Variables["tod_final_high"] = maxPlayers[0];

                if (context.Variables.TryGetValue("tod_pending_low", out var pendingObj) && pendingObj is List<string> pendingLow && pendingLow.Count > 1) {
                    context.Variables["tod_resolving"] = "low";
                    context.Variables["tod_expected_players"] = pendingLow;
                    executionService.RequestBroadcast($"[Truth or Dare] The highest roller is now {maxPlayers[0]}! We must now resolve the tie for the LOWEST score: {string.Join(", ", pendingLow)}, you have 60 seconds to /random!");
                }
                else {
                    var lowPlayer = (context.Variables.TryGetValue("tod_pending_low", out var pObj) && pObj is List<string> pL && pL.Count > 0) ? pL[0] : minPlayers[0];
                    context.Variables["tod_resolving"] = "done";
                    executionService.RequestBroadcast($"[Truth or Dare] Tie broken! The highest roller is {maxPlayers[0]}.\n{maxPlayers[0]}, you must ask Truth or Dare to {lowPlayer}!");
                }
            }
        }
        else if (resolvingState == "low") {
            if (minPlayers.Count > 1) {
                context.Variables["tod_expected_players"] = minPlayers;
                executionService.RequestBroadcast($"[Truth or Dare] Still tied for LOWEST ({minScore})! {string.Join(", ", minPlayers)}, please /random again!");
            }
            else {
                string highPlayer = context.Variables.TryGetValue("tod_final_high", out var h) ? h?.ToString() ?? maxPlayers[0] : maxPlayers[0];
                context.Variables["tod_resolving"] = "done";
                executionService.RequestBroadcast($"[Truth or Dare] Tie broken! The lowest roller is {minPlayers[0]}.\n{highPlayer}, you must ask Truth or Dare to {minPlayers[0]}!");
            }
        }
    }
}