namespace RolePlayer.UI.Hotbar;

using Microsoft.Extensions.DependencyInjection;
using RolePlayer.Core.Framework;
using RolePlayer.UI.Hotbar.Components;
using RolePlayer.UI.Hotbar.Contracts;
using RolePlayer.UI.Hotbar.Services;

public class HotbarFeature : IFeatureModule {
    public void RegisterServices(IServiceCollection services) {
        services.AddSingleton<IHotbarResolverService, HotbarResolverService>();
        services.AddSingleton<HotbarManagerComponent>();
    }
}