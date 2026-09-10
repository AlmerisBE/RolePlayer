namespace RolePlayer.UI.GameDashboard;

using Dalamud.Interface.Windowing;
using Microsoft.Extensions.DependencyInjection;
using RolePlayer.Core.Framework;
using RolePlayer.UI.Command.Contracts;
using RolePlayer.UI.GameDashboard.Commands;
using RolePlayer.UI.GameDashboard.Contracts;
using RolePlayer.UI.GameDashboard.Presenters;
using RolePlayer.UI.GameDashboard.Windows;

public class GameDashboardFeature : IFeatureModule {
    public void RegisterServices(IServiceCollection services) {
        services.AddSingleton<IGameDashboardPresenter, GameDashboardPresenter>();

        services.AddSingleton<GameDashboardWindow>();
        services.AddSingleton<Window>(provider => provider.GetRequiredService<GameDashboardWindow>());

        services.AddSingleton<ICommand, DashboardCommand>();
    }
}