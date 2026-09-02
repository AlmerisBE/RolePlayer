namespace RolePlayer.API.Penumbra;

using Microsoft.Extensions.DependencyInjection;
using RolePlayer.API.Penumbra.Providers;
using RolePlayer.Core.Framework;
using RolePlayer.UI.EmoteBrowser.Contracts;

public class PenumbraFeature : IFeatureModule {
    public void RegisterServices(IServiceCollection services) {
        services.AddSingleton<IModStateProvider, PenumbraIpcProvider>();
    }
}