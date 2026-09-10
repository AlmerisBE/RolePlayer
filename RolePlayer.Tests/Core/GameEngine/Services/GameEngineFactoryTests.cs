namespace RolePlayer.Tests.Core.GameEngine.Services;

using NSubstitute;
using RolePlayer.Core.GameEngine.Contracts;
using RolePlayer.Core.GameEngine.Services;
using System.Collections.Generic;
using Xunit;

public class GameEngineFactoryTests {
    [Fact]
    public void CreateEngine_ReturnsCorrectEngine_WhenTypeMatches() {
        var mockEngine1 = Substitute.For<IGameEngine>();
        mockEngine1.EngineType.Returns("DeathRollEngine");

        var mockEngine2 = Substitute.For<IGameEngine>();
        mockEngine2.EngineType.Returns("RiddleEngine");

        var engines = new List<IGameEngine> { mockEngine1, mockEngine2 };
        var factory = new GameEngineFactory(engines);

        var resolvedEngine = factory.CreateEngine("DeathRollEngine");

        Assert.NotNull(resolvedEngine);
        Assert.Equal("DeathRollEngine", resolvedEngine.EngineType);
    }

    [Fact]
    public void CreateEngine_ReturnsNull_WhenTypeIsUnknown() {
        var mockEngine = Substitute.For<IGameEngine>();
        mockEngine.EngineType.Returns("DeathRollEngine");

        var engines = new List<IGameEngine> { mockEngine };
        var factory = new GameEngineFactory(engines);

        var resolvedEngine = factory.CreateEngine("UnknownEngine");

        Assert.Null(resolvedEngine);
    }
}