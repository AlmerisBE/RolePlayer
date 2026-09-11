namespace RolePlayer.Tests.Core.GameEngine.Services;

using NSubstitute;
using RolePlayer.Core.GameEngine.Contracts;
using RolePlayer.Core.GameEngine.Engines;
using RolePlayer.Core.GameEngine.Services;
using System;
using Xunit;

public class GameEngineFactoryTests {
    [Fact]
    public void CreateEngine_WithValidType_ReturnsStateMachineEngine() {
        var mockServiceProvider = Substitute.For<IServiceProvider>();

        var evaluator = Substitute.For<IConditionEvaluatorService>();
        var actionService = Substitute.For<IGameActionExecutionService>();
        var engine = new StateMachineEngine(evaluator, actionService);

        mockServiceProvider.GetService(typeof(StateMachineEngine)).Returns(engine);

        var factory = new GameEngineFactory(mockServiceProvider);

        var result = factory.CreateEngine("StateMachineEngine");

        Assert.NotNull(result);
        Assert.IsType<StateMachineEngine>(result);
    }

    [Fact]
    public void CreateEngine_WithInvalidType_ReturnsNull() {
        var mockServiceProvider = Substitute.For<IServiceProvider>();
        var factory = new GameEngineFactory(mockServiceProvider);

        var result = factory.CreateEngine("UnknownEngine");

        Assert.Null(result);
    }
}