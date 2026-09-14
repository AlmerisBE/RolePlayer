namespace RolePlayer.Core.GameEngine.Contracts;

using RolePlayer.Core.GameEngine.Models;

public interface IDefaultGameTemplate {
    string FileName { get; }
    GameDefinition Build();
}