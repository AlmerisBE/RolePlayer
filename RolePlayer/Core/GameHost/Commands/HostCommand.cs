namespace RolePlayer.Core.GameHost.Commands;

using RolePlayer.Core.GameHost.Windows;
using RolePlayer.UI.Command.Contracts;

public class HostCommand : ICommand {
    private GameHostWindow window;

    public string CommandTrigger => "host";
    public string Description => "Ouvre le tableau de bord du Maître du Jeu.";

    public HostCommand(GameHostWindow window) {
        this.window = window;
    }

    public void Execute(string arguments) {
        this.window.Toggle();
    }
}