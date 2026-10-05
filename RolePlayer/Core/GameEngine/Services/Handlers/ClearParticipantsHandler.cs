namespace RolePlayer.Core.GameEngine.Services.Handlers;

using RolePlayer.Core.GameEngine.Contracts;
using RolePlayer.Core.GameEngine.Models;

public class ClearParticipantsHandler : IGameActionHandler {
    public string ActionType => "ClearParticipants";

    public void Execute(GameActionConfig action, GameSessionContext context, IGameActionExecutionService executionService) {
        context.Participants.Clear();
    }
}