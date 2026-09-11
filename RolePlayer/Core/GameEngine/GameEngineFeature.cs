namespace RolePlayer.Core.GameEngine;

using Microsoft.Extensions.DependencyInjection;
using RolePlayer.Core.Framework;
using RolePlayer.Core.GameEngine.Contracts;
using RolePlayer.Core.GameEngine.Engines;
using RolePlayer.Core.GameEngine.Services;

public class GameEngineFeature : IFeatureModule {
    public void RegisterServices(IServiceCollection services) {
        services.AddSingleton<IGameLibraryService, GameLibraryService>();
        services.AddSingleton<IGameSerializerService, GameSerializerService>();
        services.AddSingleton<IGameSessionService, GameSessionService>();
        services.AddSingleton<IChatBroadcaster, ChatBroadcaster>();
        services.AddSingleton<IGameEngineFactory, GameEngineFactory>();
        services.AddSingleton<IConditionEvaluatorService, ConditionEvaluatorService>();

        // Services nécessitant une nouvelle instance par partie (Transient)
        services.AddTransient<IGameActionExecutionService, GameActionExecutionService>();
        services.AddTransient<StateMachineEngine>();
    }
}