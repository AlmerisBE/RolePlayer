namespace RolePlayer.Tests.Core.GameEngine.Services;

using NSubstitute;
using RolePlayer.Core.GameEngine.Contracts;
using RolePlayer.Core.GameEngine.Models;
using RolePlayer.Core.GameEngine.Services;
using System.Collections.Generic;
using Xunit;

public class GameActionExecutionServiceTests {
    [Fact]
    public void Execute_ResolvesCorrectHandlerAndExecutes() {
        var mockHandler = Substitute.For<IGameActionHandler>();
        mockHandler.ActionType.Returns("MockAction");

        var handlers = new List<IGameActionHandler> { mockHandler };
        var service = new GameActionExecutionService(handlers);

        var actionConfig = new GameActionConfig { ActionType = "MockAction" };
        var context = new GameSessionContext();

        service.Execute(actionConfig, context);

        mockHandler.Received(1).Execute(actionConfig, context, service);
    }

    [Fact]
    public void Execute_DoesNothing_WhenHandlerNotFound() {
        var mockHandler = Substitute.For<IGameActionHandler>();
        mockHandler.ActionType.Returns("MockAction");

        var handlers = new List<IGameActionHandler> { mockHandler };
        var service = new GameActionExecutionService(handlers);

        var actionConfig = new GameActionConfig { ActionType = "UnknownAction" };
        var context = new GameSessionContext();

        service.Execute(actionConfig, context);

        mockHandler.DidNotReceive().Execute(Arg.Any<GameActionConfig>(), Arg.Any<GameSessionContext>(), Arg.Any<IGameActionExecutionService>());
    }

    [Fact]
    public void FormatString_ReplacesVariablesCorrectly() {
        var service = new GameActionExecutionService(new List<IGameActionHandler>());
        var context = new GameSessionContext();
        context.Variables["test_var"] = "HelloWorld";

        string result = service.FormatString("Say {Var.test_var}!", context);

        Assert.Equal("Say HelloWorld!", result);
    }
}