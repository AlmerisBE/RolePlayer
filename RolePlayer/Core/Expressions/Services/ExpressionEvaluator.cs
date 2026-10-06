namespace RolePlayer.Core.Expressions.Services;

using RolePlayer.Core.Expressions.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;

public class ExpressionEvaluator : IExpressionEvaluator {
    public bool Evaluate(string expression, Func<string, object?> variableResolver) {
        if (string.IsNullOrWhiteSpace(expression)) return true;

        string[] knownOperators = { "==", "!=", ">=", "<=", " CONTAINS ", " NOT_CONTAINS ", ">", "<", "=" };
        string leftRaw = string.Empty;
        string op = string.Empty;
        string rightRaw = string.Empty;

        foreach (var knownOp in knownOperators) {
            int idx = expression.IndexOf(knownOp, StringComparison.OrdinalIgnoreCase);
            if (idx != -1) {
                leftRaw = expression.Substring(0, idx).Trim();
                op = knownOp.Trim().ToUpperInvariant();
                rightRaw = expression.Substring(idx + knownOp.Length).Trim();
                break;
            }
        }

        if (string.IsNullOrEmpty(op)) return false;

        object? leftValue = this.ResolveValue(leftRaw, variableResolver);
        object? rightValue = this.ResolveValue(rightRaw, variableResolver);

        return this.Compare(leftValue, op, rightValue);
    }

    private object? ResolveValue(string raw, Func<string, object?> variableResolver) {
        if (string.Equals(raw, "null", StringComparison.OrdinalIgnoreCase)) return null;
        if (long.TryParse(raw, out long longVal)) return longVal;
        if (raw.StartsWith("'") && raw.EndsWith("'")) return raw.Trim('\'');
        if (raw.StartsWith("\"") && raw.EndsWith("\"")) return raw.Trim('"');

        string cleanVarName = raw;
        if (cleanVarName.StartsWith("{") && cleanVarName.EndsWith("}")) {
            cleanVarName = cleanVarName.Substring(1, cleanVarName.Length - 2);
        }

        object? resolved = variableResolver(cleanVarName);
        return resolved ?? raw;
    }

    private bool Compare(object? left, string op, object? right) {
        if (op == "CONTAINS" && left is IEnumerable<string> list && right is string strRight) return list.Contains(strRight, StringComparer.OrdinalIgnoreCase);
        if (op == "NOT_CONTAINS" && left is IEnumerable<string> listNot && right is string strRightNot) return !listNot.Contains(strRightNot, StringComparer.OrdinalIgnoreCase);

        if (left == null || right == null) {
            if (op == "==" || op == "=") return left == right;
            if (op == "!=") return left != right;
            return false;
        }

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

    public bool Validate(string expression, out string errorMessage) {
        errorMessage = string.Empty;
        if (string.IsNullOrWhiteSpace(expression)) {
            errorMessage = "Condition expression is empty.";
            return false;
        }

        string[] knownOperators = { "==", "!=", ">=", "<=", " CONTAINS ", " NOT_CONTAINS ", ">", "<", "=" };
        bool hasOperator = false;

        foreach (var knownOp in knownOperators) {
            if (expression.IndexOf(knownOp, StringComparison.OrdinalIgnoreCase) != -1) {
                hasOperator = true;
                break;
            }
        }

        if (!hasOperator) {
            errorMessage = "Expression is missing a valid operator (e.g., ==, !=, >, <).";
            return false;
        }

        if (!expression.Contains("{") || !expression.Contains("}")) {
            errorMessage = "Expression must contain at least one variable enclosed in curly braces { } (e.g., {Player.Job}).";
            return false;
        }

        return true;
    }
}