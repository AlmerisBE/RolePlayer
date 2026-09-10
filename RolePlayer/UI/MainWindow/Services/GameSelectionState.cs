namespace RolePlayer.UI.MainWindow.Services;

using RolePlayer.Core.GameEngine.Models;
using RolePlayer.UI.MainWindow.Contracts;

public class GameSelectionState : IGameSelectionState {
    public GameDefinition? SelectedGame { get; set; }
    public bool IsCreatingNew { get; set; }
}