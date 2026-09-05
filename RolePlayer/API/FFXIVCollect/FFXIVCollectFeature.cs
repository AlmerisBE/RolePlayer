namespace RolePlayer.API.FFXIVCollect;

using Microsoft.Extensions.DependencyInjection;
using RolePlayer.API.FFXIVCollect.Providers;
using RolePlayer.Core.Framework;
using RolePlayer.UI.EmoteBrowser.Contracts;

public class FFXIVCollectFeature : IFeatureModule {
    public void RegisterServices(IServiceCollection services) {
        services.AddSingleton<IUnlockSourceProvider, FFXIVCollectUnlockSourceProvider>();
    }
}