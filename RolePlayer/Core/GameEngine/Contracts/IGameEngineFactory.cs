namespace RolePlayer.Core.GameEngine.Contracts;

public interface IGameEngineFactory {
    IGameEngine? CreateEngine(string engineType);
}