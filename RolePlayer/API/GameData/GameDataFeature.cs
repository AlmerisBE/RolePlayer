namespace RolePlayer.API.GameData;

using Microsoft.Extensions.DependencyInjection;
using RolePlayer.API.GameData.Commands;
using RolePlayer.API.GameData.Providers;
using RolePlayer.API.Penumbra.Contracts;
using RolePlayer.Core.Framework;
using RolePlayer.UI.Command.Contracts;
using RolePlayer.UI.EmoteBrowser.Contracts;

public class GameDataFeature : IFeatureModule {
    public void RegisterServices(IServiceCollection services) {
        services.AddSingleton<IEmoteRepository, LuminaEmoteProvider>();
        services.AddSingleton<IPlayerStateProvider, PlayerStateProvider>();

        // Enregistrement en tant que classe concrète (Fallback)
        services.AddSingleton<LuminaUnlockSourceProvider>();

        services.AddSingleton<IEmotePathProvider, LuminaEmotePathProvider>();
        services.AddSingleton<IEmoteDebugService, LuminaEmoteDebugService>();
        services.AddSingleton<IEmoteExecutionService, EmoteExecutionProvider>();

        services.AddSingleton<ICommand, DumpCommand>();
    }
}