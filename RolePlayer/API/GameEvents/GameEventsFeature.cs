namespace RolePlayer.API.GameEvents;

using Microsoft.Extensions.DependencyInjection;
using RolePlayer.API.GameEvents.Services;
using RolePlayer.Core.Framework;
using RolePlayer.Core.GameEngine.Contracts;
using RolePlayer.Core.GameEngine.Services;

public class GameEventsFeature : IFeatureModule {
    public void RegisterServices(IServiceCollection services) {
        services.AddSingleton<IGameEventWatcher, ChatWatcher>();
        services.AddSingleton<IGameEventWatcher, EmoteWatcher>();

        services.AddSingleton<IChatBroadcaster, ChatBroadcaster>();
    }
}