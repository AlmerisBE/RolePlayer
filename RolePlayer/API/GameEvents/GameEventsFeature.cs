namespace RolePlayer.API.GameEvents;

using Microsoft.Extensions.DependencyInjection;
using RolePlayer.API.GameEvents.Contracts;
using RolePlayer.API.GameEvents.Services;
using RolePlayer.Core.Framework;
using RolePlayer.Core.GameEngine.Contracts;

public class GameEventsFeature : IFeatureModule {
    public void RegisterServices(IServiceCollection services) {
        services.AddSingleton<IPlayerNameNormalizer, PlayerNameNormalizer>();
        services.AddSingleton<IDiceRollParser, DiceRollParser>();

        services.AddSingleton<IGameEventWatcher, ChatWatcher>();
        services.AddSingleton<IGameEventWatcher, EmoteWatcher>();
    }
}