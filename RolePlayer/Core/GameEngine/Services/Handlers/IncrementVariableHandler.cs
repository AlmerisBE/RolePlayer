namespace RolePlayer.Core.GameEngine.Services.Handlers;

using RolePlayer.Core.GameEngine.Contracts;
using RolePlayer.Core.GameEngine.Models;

public class IncrementVariableHandler : IGameActionHandler {
    public string ActionType => "IncrementVariable";

    public void Execute(GameActionConfig action, GameSessionContext context, IGameActionExecutionService executionService) {
        if (!action.Parameters.TryGetValue("TargetVar", out var rawTargetVar) || string.IsNullOrWhiteSpace(rawTargetVar)) return;
        if (!action.Parameters.TryGetValue("Value", out var rawValue)) return;

        string targetVar = executionService.FormatString(rawTargetVar, context);
        string formattedValue = executionService.FormatString(rawValue, context);

        int increment = int.TryParse(formattedValue, out int inc) ? inc : 1;
        int current = 0;

        if (context.Variables.TryGetValue(targetVar, out var val)) {
            if (val is int cInt) current = cInt;
            else if (int.TryParse(val?.ToString(), out int pInt)) current = pInt;
        }

        context.Variables[targetVar] = current + increment;
    }
}