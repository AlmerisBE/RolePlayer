namespace RolePlayer.Core.GameEngine.Services.Handlers;

using RolePlayer.Core.GameEngine.Contracts;
using RolePlayer.Core.GameEngine.Models;

public class StopGameHandler : IGameActionHandler {
    public string ActionType => "StopGame";

    public void Execute(GameActionConfig action, GameSessionContext context, IGameActionExecutionService executionService) {
        executionService.RequestGameStop();
    }
}