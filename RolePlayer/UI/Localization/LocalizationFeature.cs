using Microsoft.Extensions.DependencyInjection;
using RolePlayer.UI.Localization.Contracts;
using RolePlayer.UI.Localization.Services;
using RolePlayer.Core.Framework;

namespace RolePlayer.UI.Localization;

public class LocalizationFeature : IFeatureModule {
    public void RegisterServices(IServiceCollection services) {
        services.AddSingleton<ILocalizationService, LocalizationService>();
    }
}