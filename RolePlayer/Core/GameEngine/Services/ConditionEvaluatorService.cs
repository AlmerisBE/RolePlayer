namespace RolePlayer.Core.GameEngine.Services;

using RolePlayer.Core.GameEngine.Contracts;
using RolePlayer.Core.GameEngine.Models;
using System;
using System.Collections.Generic;
using System.Linq;

public class ConditionEvaluatorService : IConditionEvaluatorService {
    public bool EvaluateAll(IEnumerable<string> expressions, GameSessionContext context) {
        if (expressions == null || !expressions.Any()) return true;

        foreach (var expr in expressions) {
            if (!this.Evaluate(expr, context)) return false;
        }

        return true;
    }

    public bool Evaluate(string expression, GameSessionContext context) {
        if (string.IsNullOrWhiteSpace(expression)) return true;

        var parts = expression.Split(' ', 3, StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length != 3) return false;

        string leftRaw = parts[0];
        string op = parts[1].ToUpperInvariant();
        string rightRaw = parts[2];

        object? leftValue = this.ResolveValue(leftRaw, context);
        object? rightValue = this.ResolveValue(rightRaw, context);

        return this.Compare(leftValue, op, rightValue);
    }

    private object? ResolveValue(string raw, GameSessionContext context) {
        if (long.TryParse(raw, out long longVal)) return longVal;

        if (raw.StartsWith("'") && raw.EndsWith("'")) return raw.Trim('\'');
        if (raw.StartsWith("\"") && raw.EndsWith("\"")) return raw.Trim('"');

        if (raw.Equals("Participants.Count", StringComparison.OrdinalIgnoreCase)) return context.Participants.Count;
        if (raw.Equals("Participants", StringComparison.OrdinalIgnoreCase)) return context.Participants;

        if (raw.StartsWith("Var.", StringComparison.OrdinalIgnoreCase)) {
            string varName = raw.Substring(4);
            if (context.Variables.TryGetValue(varName, out var val)) return val;
            return null;
        }

        if (raw.StartsWith("Event.", StringComparison.OrdinalIgnoreCase)) {
            if (context.CurrentEvent == null) return null;

            string prop = raw.Substring(6).ToLowerInvariant();

            if (prop == "sender") return context.CurrentEvent.Sender;

            if (context.CurrentEvent is ChatGameEvent chat) {
                if (prop == "message") return chat.Message;
            }
            else if (context.CurrentEvent is DiceRollGameEvent dice) {
                if (prop == "roll") return dice.Roll;
                if (prop == "outof") return dice.OutOf;
            }
            else if (context.CurrentEvent is EmoteGameEvent emote) {
                if (prop == "emoteid") return emote.EmoteId;
            }
        }

        return raw;
    }

    private bool Compare(object? left, string op, object? right) {
        if (op == "CONTAINS" && left is IEnumerable<string> list && right is string strRight) {
            return list.Contains(strRight, StringComparer.OrdinalIgnoreCase);
        }

        if (op == "NOT_CONTAINS" && left is IEnumerable<string> listNot && right is string strRightNot) {
            return !listNot.Contains(strRightNot, StringComparer.OrdinalIgnoreCase);
        }

        if (left == null || right == null) return left == right && (op == "==" || op == "=");

        if (long.TryParse(left.ToString(), out long lVal) && long.TryParse(right.ToString(), out long rVal)) {
            return op switch {
                "==" or "=" => lVal == rVal,
                "!=" => lVal != rVal,
                ">" => lVal > rVal,
                ">=" => lVal >= rVal,
                "<" => lVal < rVal,
                "<=" => lVal <= rVal,
                _ => false
            };
        }

        string leftStr = left.ToString() ?? string.Empty;
        string rightStr = right.ToString() ?? string.Empty;

        return op switch {
            "==" or "=" => leftStr.Equals(rightStr, StringComparison.OrdinalIgnoreCase),
            "!=" => !leftStr.Equals(rightStr, StringComparison.OrdinalIgnoreCase),
            _ => false
        };
    }
}