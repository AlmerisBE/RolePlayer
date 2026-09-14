namespace RolePlayer.Core.Macros;

using Microsoft.Extensions.DependencyInjection;
using RolePlayer.Core.Framework;
using RolePlayer.Core.Macros.Contracts;
using RolePlayer.Core.Macros.Services;

public class MacrosFeature : IFeatureModule {
    public void RegisterServices(IServiceCollection services) {
        services.AddSingleton<IMacroManagementService, MacroManagementService>();
        services.AddSingleton<IMacroExecutionService, MacroExecutionService>();
    }
}