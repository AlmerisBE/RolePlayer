using Microsoft.Extensions.DependencyInjection;

namespace RolePlayer.Core.Framework;

public interface IFeatureModule {
    void RegisterServices(IServiceCollection services);
}