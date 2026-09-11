namespace RolePlayer.UI.MainWindow;

using Dalamud.Interface.Windowing;
using Microsoft.Extensions.DependencyInjection;
using RolePlayer.Core.Framework;
using RolePlayer.UI.Command.Contracts;
using RolePlayer.UI.EmoteBrowser.Contracts;
using RolePlayer.UI.Localization.Contracts;
using RolePlayer.UI.MainWindow.Commands;
using RolePlayer.UI.MainWindow.Components;
using RolePlayer.UI.MainWindow.Components.Games;
using RolePlayer.UI.MainWindow.Components.Macros;
using RolePlayer.UI.MainWindow.Contracts;
using RolePlayer.UI.MainWindow.Providers;
using RolePlayer.UI.MainWindow.Services;
using RolePlayer.UI.MainWindow.Tabs;
using RolePlayer.UI.MainWindow.Tabs.SubTabs;
using RolePlayer.UI.MainWindow.Windows;

public class MainWindowFeature : IFeatureModule {
    public void RegisterServices(IServiceCollection services) {
        services.AddSingleton<MainWindow>();

        services.AddSingleton<TabManagerComponent>();
        services.AddSingleton<StatusBarComponent>();
        services.AddSingleton<MainLayoutComponent>();

        services.AddSingleton<GeneralConfigSubTab>();
        services.AddSingleton<HotbarConfigSubTab>();
        services.AddSingleton<GroupsConfigSubTab>();
        services.AddSingleton<TagsConfigSubTab>();
        services.AddSingleton<ContextsConfigSubTab>();

        // Injection d'état pour les Macros
        services.AddSingleton<IMacroSelectionState, MacroSelectionState>();

        // Sous-composants Macro
        services.AddSingleton<MacroCreationComponent>();
        services.AddSingleton<MacroSearchComponent>();
        services.AddSingleton<MacroDeleteModalComponent>();
        services.AddSingleton<MacroContextMenuComponent>();
        services.AddSingleton<MacroListComponent>();
        services.AddSingleton<MacroEditorPanelComponent>();

        services.AddSingleton<Window>(provider => provider.GetRequiredService<MainWindow>());
        services.AddSingleton<ICommand, OpenMainWindowCommand>();

        services.AddSingleton<GamesListComponent>();

        services.AddSingleton<IEmoteBrowserTab, ConfigurationTab>();
        services.AddSingleton<IEmoteBrowserTab, AboutTab>();
        services.AddSingleton<IEmoteBrowserTab, MacrosTab>();
        services.AddSingleton<IEmoteBrowserTab, GamesTab>();

        services.AddSingleton<ILocalizationProvider, MainWindowLocalizationProvider>();
    }
}