namespace RolePlayer.Core.GameHost;

using Microsoft.Extensions.DependencyInjection;
using RolePlayer.Core.Framework;
using RolePlayer.Core.GameHost.Contracts;
using RolePlayer.Core.GameHost.Services;

public class GameHostFeature : IFeatureModule {
    public void RegisterServices(IServiceCollection services) {
        services.AddSingleton<IGameLibraryService, GameLibraryService>();
    }
}