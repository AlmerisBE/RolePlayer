namespace RolePlayer.UI.MainWindow.Commands;

using RolePlayer.UI.Command.Contracts;
using RolePlayer.UI.MainWindow.Contracts;

public class ConfigCommand : ICommand {
    private IMainWindow mainWindow;

    public string CommandTrigger => "config";
    public string Description => "Ouvre la fenêtre principale sur l'onglet de configuration.";

    public ConfigCommand(IMainWindow mainWindow) {
        this.mainWindow = mainWindow;
    }

    public void Execute(string arguments) {
        this.mainWindow.OpenConfig();
    }
}