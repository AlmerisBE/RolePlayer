namespace RolePlayer.Core.GameEngine;

using Microsoft.Extensions.DependencyInjection;
using RolePlayer.Core.Framework;
using RolePlayer.Core.GameEngine.Contracts;
using RolePlayer.Core.GameEngine.Engines;
using RolePlayer.Core.GameEngine.Services;
using RolePlayer.Core.GameEngine.Services.Handlers;
using RolePlayer.Core.GameEngine.Templates;

public class GameEngineFeature : IFeatureModule {
    public void RegisterServices(IServiceCollection services) {
        services.AddSingleton<IDefaultGameTemplate, DeathRollTemplate>();
        services.AddSingleton<IDefaultGameTemplate, RiddlesTemplate>();
        services.AddSingleton<IDefaultGameTemplate, TruthOrDareTemplate>();
        services.AddSingleton<IDefaultGameTemplate, BlackjackTemplate>();
        services.AddSingleton<IDefaultGameTemplate, NaughtyGamesTemplate>();

        services.AddSingleton<IGameTemplateProvider, DefaultGameTemplateProvider>();

        services.AddSingleton<IGameLibraryService, GameLibraryService>();
        services.AddSingleton<IGameSerializerService, GameSerializerService>();
        services.AddSingleton<IGameSessionService, GameSessionService>();
        services.AddSingleton<IChatBroadcaster, ChatBroadcaster>();
        services.AddSingleton<IGameEngineFactory, GameEngineFactory>();
        services.AddSingleton<IConditionEvaluatorService, ConditionEvaluatorService>();

        // Register Action Handlers
        services.AddTransient<IGameActionHandler, ClearParticipantsHandler>();
        services.AddTransient<IGameActionHandler, BroadcastMessageHandler>();
        services.AddTransient<IGameActionHandler, EvaluateMinMaxHandler>();

        services.AddTransient<IGameActionExecutionService, GameActionExecutionService>();
        services.AddTransient<StateMachineEngine>();
    }
}