namespace RolePlayer.Core.GameEngine.Contracts;

using RolePlayer.Core.GameEngine.Models;

public interface IGameActionHandler {
    string ActionType { get; }
    void Execute(GameActionConfig action, GameSessionContext context, IGameActionExecutionService executionService);
}