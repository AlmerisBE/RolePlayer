namespace RolePlayer.Core.GameEngine.Services.Handlers;

using RolePlayer.Core.GameEngine.Contracts;
using RolePlayer.Core.GameEngine.Models;

public class BroadcastMessageHandler : IGameActionHandler {
    public string ActionType => "BroadcastMessage";

    public void Execute(GameActionConfig action, GameSessionContext context, IGameActionExecutionService executionService) {
        if (!action.Parameters.TryGetValue("Message", out var rawMessage) || string.IsNullOrWhiteSpace(rawMessage)) return;

        string formattedMessage = executionService.FormatString(rawMessage, context);
        executionService.RequestBroadcast(formattedMessage);
    }
}