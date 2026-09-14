namespace RolePlayer.Core.Emotes;

using Microsoft.Extensions.DependencyInjection;
using RolePlayer.Core.Emotes.Contracts;
using RolePlayer.Core.Emotes.Services;
using RolePlayer.Core.Framework;

public class EmotesFeature : IFeatureModule {
    public void RegisterServices(IServiceCollection services) {
        services.AddSingleton<EmoteCacheService>();
        services.AddSingleton<IEmoteCache>(p => p.GetRequiredService<EmoteCacheService>());
    }
}