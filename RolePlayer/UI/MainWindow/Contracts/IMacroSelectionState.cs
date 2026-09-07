namespace RolePlayer.UI.MainWindow.Contracts;

using RolePlayer.Core.Macros.Models;

public interface IMacroSelectionState {
    RoleplayMacro? SelectedMacro { get; set; }
}