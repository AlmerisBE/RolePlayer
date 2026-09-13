namespace RolePlayer.Core.Configuration;

using Microsoft.Extensions.DependencyInjection;
using RolePlayer.Core.Configuration.Contracts;
using RolePlayer.Core.Configuration.Services;
using RolePlayer.Core.Framework;

public class ConfigurationFeature : IFeatureModule {
    public void RegisterServices(IServiceCollection services) {
        services.AddSingleton<IConfigurationService, ConfigurationService>();
        services.AddSingleton<IContextManagementService, ContextManagementService>();
    }
}