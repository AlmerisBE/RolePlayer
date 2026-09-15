namespace RolePlayer.UI.Common;

using Microsoft.Extensions.DependencyInjection;
using RolePlayer.Core.Framework;
using RolePlayer.UI.Common.Components;
using RolePlayer.UI.Common.Contracts;

public class CommonUIFeature : IFeatureModule {
    public void RegisterServices(IServiceCollection services) {
        services.AddSingleton<IHelpMarkerComponent, HelpMarkerComponent>();
    }
}