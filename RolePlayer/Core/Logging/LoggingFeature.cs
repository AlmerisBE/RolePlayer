using Microsoft.Extensions.DependencyInjection;
using RolePlayer.Core.Logging.Contracts;
using RolePlayer.Core.Logging.Services;
using RolePlayer.Core.Framework;

namespace RolePlayer.Core.Logging;

public class LoggingFeature : IFeatureModule {
    public void RegisterServices(IServiceCollection services) {
        services.AddSingleton<ILoggerService, LoggerService>();
    }
}