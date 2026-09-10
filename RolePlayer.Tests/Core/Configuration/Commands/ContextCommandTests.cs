namespace RolePlayer.Tests.Core.Configuration.Commands;

using NSubstitute;
using RolePlayer.Core.Configuration.Commands;
using RolePlayer.Core.Configuration.Contracts;
using Xunit;

public class ContextCommandTests {
    [Fact]
    public void Execute_WithValidArgument_CallsSwitchContextByName() {
        var mockContextService = Substitute.For<IContextManagementService>();
        var command = new ContextCommand(mockContextService);

        command.Execute("Tavern");

        mockContextService.Received(1).SwitchContextByName("Tavern");
    }

    [Fact]
    public void Execute_WithEmptyArgument_DoesNothing() {
        var mockContextService = Substitute.For<IContextManagementService>();
        var command = new ContextCommand(mockContextService);

        command.Execute("   ");

        mockContextService.DidNotReceive().SwitchContextByName(Arg.Any<string>());
    }
}