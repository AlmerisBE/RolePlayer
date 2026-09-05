namespace RolePlayer;

using Dalamud.Interface.Windowing;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;
using Microsoft.Extensions.DependencyInjection;
using RolePlayer.Core.Framework;
using RolePlayer.UI.Command.Services;
using RolePlayer.UI.Themes.Contracts;

public sealed class RolePlayerPlugin : IDalamudPlugin {
    public string Name => "RolePlayer";

    private ServiceProvider serviceProvider;
    private IDalamudPluginInterface pluginInterface;
    private WindowSystem windowSystem;
    private IThemeManagementService themeService;

    public RolePlayerPlugin(
        IDalamudPluginInterface pluginInterface,
        IChatGui chatGui,
        ICommandManager commandManager,
        IClientState clientState,
        IPluginLog pluginLog,
        IDataManager dataManager,
        IObjectTable objectTable,
        IGameInteropProvider interopProvider,
        ITextureProvider textureProvider,
        IFramework framework,
        ICondition condition,
        IKeyState keyState) {

        this.pluginInterface = pluginInterface;
        this.windowSystem = new WindowSystem("RolePlayer");

        var services = new ServiceCollection();

        services.AddSingleton(this.pluginInterface);
        services.AddSingleton(chatGui);
        services.AddSingleton(commandManager);
        services.AddSingleton(clientState);
        services.AddSingleton(pluginLog);
        services.AddSingleton(dataManager);
        services.AddSingleton(objectTable);
        services.AddSingleton(interopProvider);
        services.AddSingleton(textureProvider);
        services.AddSingleton(framework);
        services.AddSingleton(condition);
        services.AddSingleton(keyState);

        services.AddPluginFeatures();

        this.serviceProvider = services.BuildServiceProvider();
        this.serviceProvider.GetRequiredService<CommandDispatcher>();
        this.themeService = this.serviceProvider.GetRequiredService<IThemeManagementService>();

        var windows = this.serviceProvider.GetServices<Window>();
        foreach (var window in windows) {
            this.windowSystem.AddWindow(window);
        }

        this.pluginInterface.UiBuilder.Draw += this.OnDraw;
        this.pluginInterface.UiBuilder.OpenConfigUi += this.OnOpenConfigUi;
        this.pluginInterface.UiBuilder.OpenMainUi += this.OnOpenMainUi;

        var hotkeyService = this.serviceProvider.GetRequiredService<RolePlayer.UI.Input.Contracts.IHotkeyService>();
        hotkeyService.OnHotkeyPressed += this.OnOpenMainUi;
    }

    private void OnDraw() {
        this.themeService.PushTheme();
        try {
            this.windowSystem.Draw();
        }
        finally {
            this.themeService.PopTheme();
        }
    }

    private void OnOpenConfigUi() {
        var mainWindow = this.serviceProvider.GetService<RolePlayer.UI.MainWindow.Windows.MainWindow>();
        if (mainWindow != null) mainWindow.OpenConfig();
    }

    private void OnOpenMainUi() {
        var mainWindow = this.serviceProvider.GetService<RolePlayer.UI.MainWindow.Windows.MainWindow>();
        if (mainWindow != null) mainWindow.Toggle();
    }

    public void Dispose() {
        this.pluginInterface.UiBuilder.Draw -= this.OnDraw;
        this.pluginInterface.UiBuilder.OpenConfigUi -= this.OnOpenConfigUi;
        this.pluginInterface.UiBuilder.OpenMainUi -= this.OnOpenMainUi;

        this.windowSystem.RemoveAllWindows();
        this.serviceProvider.Dispose();
    }
}