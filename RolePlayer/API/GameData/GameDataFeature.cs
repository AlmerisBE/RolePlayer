namespace RolePlayer.API.GameData;

using Microsoft.Extensions.DependencyInjection;
using RolePlayer.API.GameData.Providers;
using RolePlayer.API.Penumbra.Contracts;
using RolePlayer.Core.Framework;
using RolePlayer.UI.EmoteBrowser.Contracts;

public class GameDataFeature : IFeatureModule {
    public void RegisterServices(IServiceCollection services) {
        services.AddSingleton<IEmoteRepository, LuminaEmoteProvider>();
        services.AddSingleton<IPlayerStateProvider, PlayerStateProvider>();
        services.AddSingleton<IUnlockSourceProvider, MockUnlockSourceProvider>();
        services.AddSingleton<IEmotePathProvider, LuminaEmotePathProvider>();
        services.AddSingleton<IEmoteDebugService, LuminaEmoteDebugService>();
    }
}