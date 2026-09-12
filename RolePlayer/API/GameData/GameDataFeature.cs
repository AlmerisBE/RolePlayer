namespace RolePlayer.API.GameData;

using Microsoft.Extensions.DependencyInjection;
using RolePlayer.API.GameData.Commands;
using RolePlayer.API.GameData.Providers;
using RolePlayer.API.Penumbra.Contracts;
using RolePlayer.Core.Emotes.Contracts;
using RolePlayer.Core.Framework;
using RolePlayer.UI.Command.Contracts;
using RolePlayer.UI.EmoteBrowser.Contracts;
using RolePlayer.UI.MainWindow.Contracts;

public class GameDataFeature : IFeatureModule {
    public void RegisterServices(IServiceCollection services) {
        services.AddSingleton<IRawEmoteRepository, LuminaEmoteProvider>();

        services.AddSingleton<PlayerStateProvider>();
        services.AddSingleton<IPlayerStateProvider>(p => p.GetRequiredService<PlayerStateProvider>());
        services.AddSingleton<IPlayerUnlockState>(p => p.GetRequiredService<PlayerStateProvider>());

        services.AddSingleton<LuminaUnlockSourceProvider>();
        services.AddSingleton<IEmotePathProvider, LuminaEmotePathProvider>();
        services.AddSingleton<IEmoteDebugService, LuminaEmoteDebugService>();
        services.AddSingleton<IEmoteExecutionService, EmoteExecutionProvider>();
        services.AddSingleton<IAutoTranslateService, AutoTranslateProvider>();

        services.AddSingleton<ICommand, DumpCommand>();
    }
}