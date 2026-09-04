namespace RolePlayer.UI.Input;

using Microsoft.Extensions.DependencyInjection;
using RolePlayer.Core.Framework;
using RolePlayer.UI.Input.Contracts;
using RolePlayer.UI.Input.Services;

public class InputFeature : IFeatureModule {
    public void RegisterServices(IServiceCollection services) {
        services.AddSingleton<IHotkeyService, HotkeyService>();
    }
}