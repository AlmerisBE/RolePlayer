namespace RolePlayer.Core.GameEngine.Services.Handlers;

using RolePlayer.Core.GameEngine.Contracts;
using RolePlayer.Core.GameEngine.Models;

public class AdvanceStageHandler : IGameActionHandler {
    public string ActionType => "AdvanceStage";

    public void Execute(GameActionConfig action, GameSessionContext context, IGameActionExecutionService executionService) {
        executionService.RequestStageAdvance();
    }
}