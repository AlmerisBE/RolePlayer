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
            if (varName.Equals("Participants.Count", StringComparison.OrdinalIgnoreCase)) return context.Participants.Count;
            if (varName.Equals("Participants", StringComparison.OrdinalIgnoreCase)) return context.Participants;

            if (varName.StartsWith("Var.", StringComparison.OrdinalIgnoreCase)) {
                string v = varName.Substring(4);
                if (context.Variables.TryGetValue(v, out var val)) return val;
                return null;
            }

            if (varName.StartsWith("Event.", StringComparison.OrdinalIgnoreCase)) {
                if (context.CurrentEvent == null) return null;
                string prop = varName.Substring(6).ToLowerInvariant();

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