namespace RolePlayer.Core.GameEngine.Services;

using Microsoft.Extensions.DependencyInjection;
using RolePlayer.Core.GameEngine.Contracts;
using RolePlayer.Core.GameEngine.Engines;
using System;

public class GameEngineFactory : IGameEngineFactory {
    private IServiceProvider serviceProvider;

    public GameEngineFactory(IServiceProvider serviceProvider) {
        this.serviceProvider = serviceProvider;
    }

    public IGameEngine? CreateEngine(string engineType) {
        if (string.IsNullOrWhiteSpace(engineType)) return null;

        // Actuellement, le StateMachineEngine est notre moteur universel
        if (engineType.Equals("StateMachineEngine", StringComparison.OrdinalIgnoreCase) ||
            engineType.Equals("DeathRollEngine", StringComparison.OrdinalIgnoreCase)) {
            return this.serviceProvider.GetRequiredService<StateMachineEngine>();
        }

        return null;
    }
}