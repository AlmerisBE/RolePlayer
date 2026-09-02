namespace RolePlayer.UI.MainWindow;

using Dalamud.Interface.Windowing;
using Microsoft.Extensions.DependencyInjection;
using RolePlayer.Core.Framework;
using RolePlayer.UI.Command.Contracts;
using RolePlayer.UI.EmoteBrowser.Contracts;
using RolePlayer.UI.MainWindow.Commands;
using RolePlayer.UI.MainWindow.Tabs;
using RolePlayer.UI.MainWindow.Windows;

public class MainWindowFeature : IFeatureModule {
    public void RegisterServices(IServiceCollection services) {
        // Enregistre la MainWindow concrète pour la commande
        services.AddSingleton<MainWindow>();

        // Enregistre en tant que Window générique pour que le Plugin l'ajoute au WindowSystem
        services.AddSingleton<Window>(provider => provider.GetRequiredService<MainWindow>());

        // Ajoute la commande d'ouverture
        services.AddSingleton<ICommand, OpenMainWindowCommand>();

        services.AddSingleton<IEmoteBrowserTab, ConfigurationTab>();
    }
}