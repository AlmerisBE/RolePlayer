using Microsoft.Extensions.DependencyInjection;
using RolePlayer.UI.Command.Services;
using RolePlayer.Core.Framework;

namespace RolePlayer.UI.Command;

public class CommandFeature : IFeatureModule {
    public void RegisterServices(IServiceCollection services) {
        services.AddSingleton<CommandDispatcher>();
    }
}