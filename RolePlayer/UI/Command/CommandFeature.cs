using Microsoft.Extensions.DependencyInjection;
using RolePlayer.Core.Framework;
using RolePlayer.UI.Command.Commands;
using RolePlayer.UI.Command.Contracts;
using RolePlayer.UI.Command.Services;

namespace RolePlayer.UI.Command;

public class CommandFeature : IFeatureModule {
    public void RegisterServices(IServiceCollection services) {
        services.AddSingleton<CommandDispatcher>();

        services.AddSingleton<ICommand, ContextCommand>();
        services.AddSingleton<ICommand, ExecuteMacroCommand>();
    }
}