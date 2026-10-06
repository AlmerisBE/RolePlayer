namespace RolePlayer.Core.Expressions.Contracts;

using System;

public interface IExpressionEvaluator {
    bool Evaluate(string expression, Func<string, object?> variableResolver);
}