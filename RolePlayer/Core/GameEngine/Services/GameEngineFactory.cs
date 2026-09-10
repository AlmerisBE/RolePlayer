namespace RolePlayer.Core.GameEngine.Services;

using RolePlayer.Core.GameEngine.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;

public class GameEngineFactory : IGameEngineFactory {
    private IEnumerable<IGameEngine> availableEngines;

    public GameEngineFactory(IEnumerable<IGameEngine> availableEngines) {
        this.availableEngines = availableEngines;
    }

    public IGameEngine? CreateEngine(string engineType) {
        if (string.IsNullOrWhiteSpace(engineType)) return null;

        return this.availableEngines.FirstOrDefault(e => e.EngineType.Equals(engineType, StringComparison.OrdinalIgnoreCase));
    }
}