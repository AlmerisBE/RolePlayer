namespace RolePlayer.Core.GameEngine;

using Microsoft.Extensions.DependencyInjection;
using RolePlayer.Core.Framework;
using RolePlayer.Core.GameEngine.Contracts;
using RolePlayer.Core.GameEngine.Services;

public class GameEngineFeature : IFeatureModule {
    public void RegisterServices(IServiceCollection services) {
        services.AddSingleton<IGameLibraryService, GameLibraryService>();
        services.AddSingleton<IGameSessionService, GameSessionService>();
    }
}