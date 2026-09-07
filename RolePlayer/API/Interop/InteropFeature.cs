namespace RolePlayer.API.Interop;

using Microsoft.Extensions.DependencyInjection;
using RolePlayer.API.Interop.Contracts;
using RolePlayer.API.Interop.Services;
using RolePlayer.Core.Framework;

public class InteropFeature : IFeatureModule {
    public void RegisterServices(IServiceCollection services) {
        services.AddSingleton<INativeExecutionService, NativeExecutionService>();
    }
}