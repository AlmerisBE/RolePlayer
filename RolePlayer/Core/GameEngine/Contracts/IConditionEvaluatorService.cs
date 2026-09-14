namespace RolePlayer.Core.GameEngine.Contracts;

using RolePlayer.Core.GameEngine.Models;
using System.Collections.Generic;

public interface IConditionEvaluatorService {
    bool Evaluate(string expression, GameSessionContext context);
    bool EvaluateAll(IEnumerable<string> expressions, GameSessionContext context);
}