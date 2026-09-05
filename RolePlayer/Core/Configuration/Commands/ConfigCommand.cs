namespace RolePlayer.Core.Configuration.Commands;

using RolePlayer.UI.Command.Contracts;
using RolePlayer.UI.MainWindow.Windows;

public class ConfigCommand : ICommand {
    private MainWindow mainWindow;

    public string CommandTrigger => "config";
    public string Description => "Ouvre la fenêtre principale sur l'onglet de configuration.";

    public ConfigCommand(MainWindow mainWindow) {
        this.mainWindow = mainWindow;
    }

    public void Execute(string arguments) {
        this.mainWindow.OpenConfig();
    }
}