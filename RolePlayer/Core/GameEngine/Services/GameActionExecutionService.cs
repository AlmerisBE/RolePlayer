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
            case "BROADCASTSCORES":
                this.ExecuteBroadcastScores(action, context);
                break;
            case "ENDGAMEIFSCOREREACHED":
                this.ExecuteEndGameIfScoreReached(action, context);
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

        // Le formatage s'applique désormais au nom de la variable également
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
                result = result.Replace("{Event.OutOf}", dice.OutOf.ToString(), StringComparison.OrdinalIgnoreCase);
            }
        }

        result = result.Replace("{Participants.Count}", context.Participants.Count.ToString(), StringComparison.OrdinalIgnoreCase);

        foreach (var kvp in context.Variables) {
            result = result.Replace($"{{Var.{kvp.Key}}}", kvp.Value?.ToString() ?? string.Empty, StringComparison.OrdinalIgnoreCase);
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
}