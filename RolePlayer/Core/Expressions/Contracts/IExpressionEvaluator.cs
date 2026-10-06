namespace RolePlayer.Core.Expressions.Contracts;

using System;

public interface IExpressionEvaluator {
    bool Evaluate(string expression, Func<string, object?> variableResolver);
    bool Validate(string expression, out string errorMessage);
}