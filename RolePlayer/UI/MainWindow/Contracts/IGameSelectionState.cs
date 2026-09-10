namespace RolePlayer.UI.MainWindow.Contracts;

using RolePlayer.Core.GameEngine.Models;

public interface IGameSelectionState {
    GameDefinition? SelectedGame { get; set; }
    bool IsCreatingNew { get; set; }
}