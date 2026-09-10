namespace RolePlayer.UI.GameDashboard.Commands;

using RolePlayer.UI.Command.Contracts;
using RolePlayer.UI.GameDashboard.Windows;

public class DashboardCommand : ICommand {
    private GameDashboardWindow window;

    public string CommandTrigger => "host";
    public string Description => "Ouvre le tableau de bord du Maître du Jeu.";

    public DashboardCommand(GameDashboardWindow window) {
        this.window = window;
    }

    public void Execute(string arguments) {
        this.window.Toggle();
    }
}