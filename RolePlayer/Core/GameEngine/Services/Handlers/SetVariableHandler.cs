namespace RolePlayer.Core.GameEngine.Services.Handlers;

using RolePlayer.Core.GameEngine.Contracts;
using RolePlayer.Core.GameEngine.Models;

public class SetVariableHandler : IGameActionHandler {
    public string ActionType => "SetVariable";

    public void Execute(GameActionConfig action, GameSessionContext context, IGameActionExecutionService executionService) {
        if (!action.Parameters.TryGetValue("TargetVar", out var rawTargetVar) || string.IsNullOrWhiteSpace(rawTargetVar)) return;
        if (!action.Parameters.TryGetValue("Value", out var rawValue)) return;

        string targetVar = executionService.FormatString(rawTargetVar, context);
        string formattedValue = executionService.FormatString(rawValue, context);

        if (int.TryParse(formattedValue, out int intVal)) context.Variables[targetVar] = intVal;
        else context.Variables[targetVar] = formattedValue;
    }
}