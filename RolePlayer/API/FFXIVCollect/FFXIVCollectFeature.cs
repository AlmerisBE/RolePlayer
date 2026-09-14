namespace RolePlayer.API.FFXIVCollect;

using Microsoft.Extensions.DependencyInjection;
using RolePlayer.API.FFXIVCollect.Providers;
using RolePlayer.Core.Emotes.Contracts;
using RolePlayer.Core.Framework;

public class FFXIVCollectFeature : IFeatureModule {
    public void RegisterServices(IServiceCollection services) {
        services.AddSingleton<IUnlockSourceProvider, FFXIVCollectUnlockSourceProvider>();
    }
}