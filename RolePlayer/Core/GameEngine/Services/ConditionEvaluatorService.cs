namespace RolePlayer.Core.GameEngine.Services;

using RolePlayer.Core.Expressions.Contracts;
using RolePlayer.Core.GameEngine.Contracts;
using RolePlayer.Core.GameEngine.Models;
using System;
using System.Collections.Generic;
using System.Linq;

public class ConditionEvaluatorService : IConditionEvaluatorService {
    private IExpressionEvaluator evaluator;

    public ConditionEvaluatorService(IExpressionEvaluator evaluator) {
        this.evaluator = evaluator;
    }

    public bool EvaluateAll(IEnumerable<string> expressions, GameSessionContext context) {
        if (expressions == null || !expressions.Any()) return true;
        foreach (var expr in expressions) {
            if (!this.Evaluate(expr, context)) return false;
        }
        return true;
    }

    public bool Evaluate(string expression, GameSessionContext context) {
        return this.evaluator.Evaluate(expression, varName => {
            string resolvedVarName = varName;

            // Resolve dynamic tags nested within the variable identifier (e.g. Var.score_{Event.Sender})
            if (context.CurrentEvent != null) {
                resolvedVarName = resolvedVarName.Replace("{Event.Sender}", context.CurrentEvent.Sender, StringComparison.OrdinalIgnoreCase);

                if (context.CurrentEvent is DiceRollGameEvent dice) {
                    resolvedVarName = resolvedVarName.Replace("{Event.Roll}", dice.Roll.ToString(), StringComparison.OrdinalIgnoreCase);
                    resolvedVarName = resolvedVarName.Replace("{Event.MaxRoll}", dice.MaxRoll.ToString(), StringComparison.OrdinalIgnoreCase);
                }
            }

            if (resolvedVarName.Contains("{Var.", StringComparison.OrdinalIgnoreCase)) {
                foreach (var kvp in context.Variables) {
                    if (kvp.Value is not List<string>) {
                        resolvedVarName = resolvedVarName.Replace($"{{Var.{kvp.Key}}}", kvp.Value?.ToString() ?? string.Empty, StringComparison.OrdinalIgnoreCase);
                    }
                }
            }

            if (resolvedVarName.Equals("Participants.Count", StringComparison.OrdinalIgnoreCase)) return context.Participants.Count;
            if (resolvedVarName.Equals("Participants", StringComparison.OrdinalIgnoreCase)) return context.Participants;

            if (resolvedVarName.StartsWith("Var.", StringComparison.OrdinalIgnoreCase)) {
                string v = resolvedVarName.Substring(4);
                if (context.Variables.TryGetValue(v, out var val)) return val;
                return null;
            }

            if (resolvedVarName.StartsWith("Event.", StringComparison.OrdinalIgnoreCase)) {
                if (context.CurrentEvent == null) return null;
                string prop = resolvedVarName.Substring(6).ToLowerInvariant();

                if (prop == "sender") return context.CurrentEvent.Sender;
                if (context.CurrentEvent is ChatGameEvent chat && prop == "message") return chat.Message;
                if (context.CurrentEvent is DiceRollGameEvent dice) {
                    if (prop == "roll") return dice.Roll;
                    if (prop == "maxroll") return dice.MaxRoll;
                }
                if (context.CurrentEvent is EmoteGameEvent emote && prop == "emoteid") return emote.EmoteId;
            }

            return null;
        });
    }
}