namespace RolePlayer.UI.MainWindow.Commands;

using RolePlayer.UI.Command.Contracts;
using RolePlayer.UI.MainWindow.Windows;

public class OpenMainWindowCommand : ICommand {
    private MainWindow mainWindow;

    public string CommandTrigger => "emotes";
    public string Description => "Ouvre le navigateur d'emotes de RolePlayer.";

    public OpenMainWindowCommand(MainWindow mainWindow) {
        this.mainWindow = mainWindow;
    }

    public void Execute(string arguments) {
        this.mainWindow.Toggle();
    }
}