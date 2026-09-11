namespace RolePlayer.UI.MainWindow.Contracts;

using RolePlayer.Core.GameEngine.Models;

public interface IGameEditorWindow {
    void OpenForEditing(GameDefinition? game);
}