using Dalamud.Interface.Windowing;
using Microsoft.Extensions.DependencyInjection;
using RolePlayer.UI.Command.Contracts;
using RolePlayer.Core.Configuration.Commands;
using RolePlayer.Core.Configuration.Contracts;
using RolePlayer.Core.Configuration.Services;
using RolePlayer.Core.Configuration.UI;
using RolePlayer.Core.Framework;

namespace RolePlayer.Core.Configuration;

public class ConfigurationFeature : IFeatureModule {
    public void RegisterServices(IServiceCollection services) {
        services.AddSingleton<IConfigurationService, ConfigurationService>();

        // Window registration
        services.AddSingleton<ConfigWindow>();
        services.AddSingleton<Window>(provider => provider.GetRequiredService<ConfigWindow>());

        services.AddSingleton<ICommand, ConfigCommand>();
    }
}