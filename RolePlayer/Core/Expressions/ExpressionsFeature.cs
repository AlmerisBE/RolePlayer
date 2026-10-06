namespace RolePlayer.Core.Expressions;

using Microsoft.Extensions.DependencyInjection;
using RolePlayer.Core.Expressions.Contracts;
using RolePlayer.Core.Expressions.Services;
using RolePlayer.Core.Framework;

public class ExpressionsFeature : IFeatureModule {
    public void RegisterServices(IServiceCollection services) {
        services.AddSingleton<IExpressionEvaluator, ExpressionEvaluator>();
    }
}