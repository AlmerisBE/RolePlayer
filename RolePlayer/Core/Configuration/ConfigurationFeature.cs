namespace RolePlayer.Core.Configuration;

using Microsoft.Extensions.DependencyInjection;
using RolePlayer.Core.Configuration.Commands;
using RolePlayer.Core.Configuration.Contracts;
using RolePlayer.Core.Configuration.Services;
using RolePlayer.Core.Framework;
using RolePlayer.UI.Command.Contracts;

public class ConfigurationFeature : IFeatureModule {
    public void RegisterServices(IServiceCollection services) {
        services.AddSingleton<IConfigurationService, ConfigurationService>();
        services.AddSingleton<IContextManagementService, ContextManagementService>();

        services.AddSingleton<ICommand, ConfigCommand>();
    }
}