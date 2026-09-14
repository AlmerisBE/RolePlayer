namespace RolePlayer.UI.MainWindow.Commands;

using RolePlayer.UI.Command.Contracts;
using RolePlayer.UI.MainWindow.Contracts;

public class OpenMainWindowCommand : ICommand {
    private IMainWindow mainWindow;

    public string CommandTrigger => "emotes";
    public string Description => "Ouvre le navigateur d'emotes de RolePlayer.";

    public OpenMainWindowCommand(IMainWindow mainWindow) {
        this.mainWindow = mainWindow;
    }

    public void Execute(string arguments) {
        this.mainWindow.Toggle();
    }
}