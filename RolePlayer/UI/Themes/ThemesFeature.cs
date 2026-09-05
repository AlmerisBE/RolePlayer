namespace RolePlayer.UI.Themes;

using Microsoft.Extensions.DependencyInjection;
using RolePlayer.Core.Framework;
using RolePlayer.UI.Themes.Contracts;
using RolePlayer.UI.Themes.Services;

public class ThemesFeature : IFeatureModule {
    public void RegisterServices(IServiceCollection services) {
        services.AddSingleton<IThemeManagementService, ThemeManagementService>();
    }
}