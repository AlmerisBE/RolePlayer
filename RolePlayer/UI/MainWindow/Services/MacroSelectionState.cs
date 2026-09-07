namespace RolePlayer.UI.MainWindow.Services;

using RolePlayer.Core.Macros.Models;
using RolePlayer.UI.MainWindow.Contracts;

public class MacroSelectionState : IMacroSelectionState {
    public RoleplayMacro? SelectedMacro { get; set; }
}