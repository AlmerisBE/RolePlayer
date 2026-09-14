namespace RolePlayer.API.Penumbra;

using Microsoft.Extensions.DependencyInjection;
using RolePlayer.API.Penumbra.Providers;
using RolePlayer.Core.Emotes.Contracts;
using RolePlayer.Core.Framework;

public class PenumbraFeature : IFeatureModule {
    public void RegisterServices(IServiceCollection services) {
        services.AddSingleton<PenumbraIpcProvider>();
        services.AddSingleton<IEmoteModState>(p => p.GetRequiredService<PenumbraIpcProvider>());
    }
}