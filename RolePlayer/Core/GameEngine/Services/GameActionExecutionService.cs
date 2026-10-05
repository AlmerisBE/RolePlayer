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

    public void ExecuteAll(IEnumerable<GameActionConfig> actions, GameSessionContext context) {
        if (actions == null) return;

        foreach (var action in actions) {
            this.Execute(action, context);
        }
    }

    public void Execute(GameActionConfig action, GameSessionContext context) {
        if (action == null || string.IsNullOrWhiteSpace(action.ActionType)) return;

        string type = action.ActionType.ToUpperInvariant();

        switch (type) {
            case "REGISTERPLAYER":
                this.ExecuteRegisterPlayer(context);
                break;
            case "SETVARIABLE":
                this.ExecuteSetVariable(action, context);
                break;
            case "INCREMENTVARIABLE":
                this.ExecuteIncrementVariable(action, context);
                break;
            case "CLEARVARIABLES":
                this.ExecuteClearVariables(action, context);
                break;
            case "BROADCASTSCORES":
                this.ExecuteBroadcastScores(action, context);
                break;
            case "ENDGAMEIFSCOREREACHED":
                this.ExecuteEndGameIfScoreReached(action, context);
                break;
            case "RESOLVEBLACKJACKWINNER":
                this.ExecuteResolveBlackjackWinner(action, context);
                break;
            case "EVALUATEMINMAX":
                this.ExecuteEvaluateMinMax(action, context);
                break;
            case "EVALUATEEQUALITY":
                this.ExecuteEvaluateEquality(action, context);
                break;
            case "EVALUATETHRESHOLD":
                this.ExecuteEvaluateThreshold(action, context);
                break;
            case "ADVANCETURN":
                this.ExecuteAdvanceTurn(action, context);
                break;
            case "BROADCASTMESSAGE":
                this.ExecuteBroadcastMessage(action, context);
                break;
            case "ADVANCESTAGE":
                this.StageAdvanceRequested?.Invoke();
                break;
            case "STOPGAME":
                this.GameStopRequested?.Invoke();
                break;
        }
    }

    private void ExecuteRegisterPlayer(GameSessionContext context) {
        if (context.CurrentEvent == null || string.IsNullOrWhiteSpace(context.CurrentEvent.Sender)) return;

        string sender = context.CurrentEvent.Sender;
        if (!context.Participants.Contains(sender, StringComparer.OrdinalIgnoreCase)) {
            context.Participants.Add(sender);
        }
    }

    private void ExecuteAdvanceTurn(GameActionConfig action, GameSessionContext context) {
        if (!action.Parameters.TryGetValue("TargetVar", out var targetVar) || string.IsNullOrWhiteSpace(targetVar)) return;
        if (context.Participants.Count == 0) return;

        string currentPlayer = context.Variables.TryGetValue(targetVar, out var val) ? val?.ToString() ?? string.Empty : string.Empty;
        int currentIndex = context.Participants.FindIndex(p => p.Equals(currentPlayer, StringComparison.OrdinalIgnoreCase));

        int nextIndex = currentIndex < 0 ? 0 : (currentIndex + 1) % context.Participants.Count;
        context.Variables[targetVar] = context.Participants[nextIndex];
    }

    private void ExecuteSetVariable(GameActionConfig action, GameSessionContext context) {
        if (!action.Parameters.TryGetValue("TargetVar", out var rawTargetVar) || string.IsNullOrWhiteSpace(rawTargetVar)) return;
        if (!action.Parameters.TryGetValue("Value", out var rawValue)) return;

        string targetVar = this.FormatString(rawTargetVar, context);
        string formattedValue = this.FormatString(rawValue, context);

        if (int.TryParse(formattedValue, out int intVal)) context.Variables[targetVar] = intVal;
        else context.Variables[targetVar] = formattedValue;
    }

    private void ExecuteIncrementVariable(GameActionConfig action, GameSessionContext context) {
        if (!action.Parameters.TryGetValue("TargetVar", out var rawTargetVar) || string.IsNullOrWhiteSpace(rawTargetVar)) return;
        if (!action.Parameters.TryGetValue("Value", out var rawValue)) return;

        string targetVar = this.FormatString(rawTargetVar, context);
        string formattedValue = this.FormatString(rawValue, context);

        int increment = int.TryParse(formattedValue, out int inc) ? inc : 1;
        int current = 0;

        if (context.Variables.TryGetValue(targetVar, out var val)) {
            if (val is int cInt) current = cInt;
            else if (int.TryParse(val?.ToString(), out int pInt)) current = pInt;
        }

        context.Variables[targetVar] = current + increment;
    }

    private void ExecuteClearVariables(GameActionConfig action, GameSessionContext context) {
        if (!action.Parameters.TryGetValue("Prefix", out var prefix) || string.IsNullOrEmpty(prefix)) return;

        var keysToRemove = context.Variables.Keys.Where(k => k.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)).ToList();
        foreach (var key in keysToRemove) {
            context.Variables.Remove(key);
        }
    }

    private void ExecuteBroadcastMessage(GameActionConfig action, GameSessionContext context) {
        if (!action.Parameters.TryGetValue("Message", out var rawMessage) || string.IsNullOrWhiteSpace(rawMessage)) return;

        string formattedMessage = this.FormatString(rawMessage, context);
        this.BroadcastRequested?.Invoke(formattedMessage);
    }

    private string FormatString(string input, GameSessionContext context) {
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

    private void ExecuteBroadcastScores(GameActionConfig action, GameSessionContext context) {
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
            this.BroadcastRequested?.Invoke("[Scores] No scores recorded yet.");
            return;
        }

        scores = scores.OrderByDescending(s => s.Score).ToList();

        var scoreStrings = scores.Select((s, index) => $"{index + 1}. {s.Name} ({s.Score} pts)");
        string leaderboard = $"[Leaderboard] {string.Join(" | ", scoreStrings)}";

        this.BroadcastRequested?.Invoke(leaderboard);
    }

    private void ExecuteEndGameIfScoreReached(GameActionConfig action, GameSessionContext context) {
        if (!action.Parameters.TryGetValue("Prefix", out var prefix)) prefix = "score_";
        if (!action.Parameters.TryGetValue("TargetScore", out var rawTarget)) return;

        string targetStr = this.FormatString(rawTarget, context);
        if (!int.TryParse(targetStr, out int targetScore)) return;

        foreach (var kvp in context.Variables) {
            if (kvp.Key.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)) {
                int score = int.TryParse(kvp.Value?.ToString(), out int s) ? s : 0;
                if (score >= targetScore) {
                    string winner = kvp.Key.Substring(prefix.Length);
                    this.BroadcastRequested?.Invoke($"[Game Over] {winner} reached {targetScore} points and wins the game!");
                    this.GameStopRequested?.Invoke();
                    return;
                }
            }
        }
    }

    private void ExecuteResolveBlackjackWinner(GameActionConfig action, GameSessionContext context) {
        if (!action.Parameters.TryGetValue("ScorePrefix", out var prefix)) prefix = "score_";
        if (!action.Parameters.TryGetValue("TargetScore", out var rawTarget)) rawTarget = "21";

        string targetStr = this.FormatString(rawTarget, context);
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
            this.BroadcastRequested?.Invoke("[Game Over] Everyone busted! The house wins.");
        }
        else {
            var maxScore = validScores.Max(s => s.Score);
            var winners = validScores.Where(s => s.Score == maxScore).Select(s => s.Name).ToList();

            if (winners.Count > 1) {
                this.BroadcastRequested?.Invoke($"[Game Over] It's a tie between {string.Join(" and ", winners)} with {maxScore} points!");
            }
            else {
                this.BroadcastRequested?.Invoke($"[Game Over] {winners[0]} wins with {maxScore} points!");
            }
        }

        this.GameStopRequested?.Invoke();
    }

    private void ExecuteEvaluateMinMax(GameActionConfig action, GameSessionContext context) {
        string prefix = action.Parameters.TryGetValue("Prefix", out var pfx) ? pfx : "score_";
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

        if (scores.Count == 0) {
            this.BroadcastRequested?.Invoke("[System] No valid rolls detected for evaluation.");
            return;
        }

        int maxScore = scores.Max(s => s.Score);
        int minScore = scores.Min(s => s.Score);

        context.Variables["max_score"] = maxScore;
        context.Variables["min_score"] = minScore;
        context.Variables["max_players"] = scores.Where(s => s.Score == maxScore).Select(s => s.Name).ToList();
        context.Variables["min_players"] = scores.Where(s => s.Score == minScore).Select(s => s.Name).ToList();
    }

    private void ExecuteEvaluateEquality(GameActionConfig action, GameSessionContext context) {
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
                this.BroadcastRequested?.Invoke($"[Truth or Dare] We have a tie for the HIGHEST score ({maxScore})! {string.Join(", ", maxPlayers)}, you have 60 seconds to /random again!");
            }
            else if (minPlayers.Count > 1) {
                context.Variables["tod_resolving"] = "low";
                context.Variables["tod_expected_players"] = minPlayers;
                context.Variables["tod_final_high"] = maxPlayers[0];
                this.BroadcastRequested?.Invoke($"[Truth or Dare] We have a tie for the LOWEST score ({minScore})! {string.Join(", ", minPlayers)}, you have 60 seconds to /random again!");
            }
            else {
                context.Variables["tod_resolving"] = "done";
                this.BroadcastRequested?.Invoke($"[Truth or Dare] Highest score: {maxPlayers[0]} ({maxScore}). Lowest score: {minPlayers[0]} ({minScore}).\n{maxPlayers[0]}, you must ask Truth or Dare to {minPlayers[0]}!");
            }
        }
        else if (resolvingState == "high") {
            if (maxPlayers.Count > 1) {
                context.Variables["tod_expected_players"] = maxPlayers;
                this.BroadcastRequested?.Invoke($"[Truth or Dare] Still tied for HIGHEST ({maxScore})! {string.Join(", ", maxPlayers)}, please /random again!");
            }
            else {
                context.Variables["tod_final_high"] = maxPlayers[0];

                if (context.Variables.TryGetValue("tod_pending_low", out var pendingObj) && pendingObj is List<string> pendingLow && pendingLow.Count > 1) {
                    context.Variables["tod_resolving"] = "low";
                    context.Variables["tod_expected_players"] = pendingLow;
                    this.BroadcastRequested?.Invoke($"[Truth or Dare] The highest roller is now {maxPlayers[0]}! We must now resolve the tie for the LOWEST score: {string.Join(", ", pendingLow)}, you have 60 seconds to /random!");
                }
                else {
                    var lowPlayer = (context.Variables.TryGetValue("tod_pending_low", out var pObj) && pObj is List<string> pL && pL.Count > 0) ? pL[0] : minPlayers[0];
                    context.Variables["tod_resolving"] = "done";
                    this.BroadcastRequested?.Invoke($"[Truth or Dare] Tie broken! The highest roller is {maxPlayers[0]}.\n{maxPlayers[0]}, you must ask Truth or Dare to {lowPlayer}!");
                }
            }
        }
        else if (resolvingState == "low") {
            if (minPlayers.Count > 1) {
                context.Variables["tod_expected_players"] = minPlayers;
                this.BroadcastRequested?.Invoke($"[Truth or Dare] Still tied for LOWEST ({minScore})! {string.Join(", ", minPlayers)}, please /random again!");
            }
            else {
                string highPlayer = context.Variables.TryGetValue("tod_final_high", out var h) ? h?.ToString() ?? maxPlayers[0] : maxPlayers[0];
                context.Variables["tod_resolving"] = "done";
                this.BroadcastRequested?.Invoke($"[Truth or Dare] Tie broken! The lowest roller is {minPlayers[0]}.\n{highPlayer}, you must ask Truth or Dare to {minPlayers[0]}!");
            }
        }
    }

    private void ExecuteEvaluateThreshold(GameActionConfig action, GameSessionContext context) {
        string prefix = action.Parameters.TryGetValue("Prefix", out var pfx) ? pfx : "score_";
        string op = action.Parameters.TryGetValue("Operator", out var o) ? o : "<=";
        string rawThreshold = action.Parameters.TryGetValue("Threshold", out var r) ? r : "0";
        string passMsg = action.Parameters.TryGetValue("PassMessage", out var pm) ? pm : "{Players} matched!";
        string failMsg = action.Parameters.TryGetValue("FailMessage", out var fm) ? fm : "No one matched!";

        int threshold = int.TryParse(this.FormatString(rawThreshold, context), out int t) ? t : 0;

        var matchingPlayers = new List<string>();
        foreach (var kvp in context.Variables.Where(k => k.Key.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))) {
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
            this.BroadcastRequested?.Invoke(finalMsg);
        }
        else {
            this.BroadcastRequested?.Invoke(failMsg);
        }
    }
}