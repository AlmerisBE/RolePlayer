namespace RolePlayer.Tests.Core.Macros.Services;

using NSubstitute;
using RolePlayer.Core.Macros.Engine.Contracts;
using RolePlayer.Core.Macros.Models;
using RolePlayer.Core.Macros.Services;
using Xunit;

public class MacroExecutionServiceTests {
    [Fact]
    public void Execute_WhenMacroIsValid_CallsPlayOnEngine() {
        var mockEngine = Substitute.For<IMacroEngine>();
        var service = new MacroExecutionService(mockEngine);
        var macro = new RoleplayMacro { Content = "/bow" };

        service.Execute(macro);

        mockEngine.Received(1).Play(macro);
    }

    [Fact]
    public void Execute_WhenMacroContentIsEmpty_CallsPlayOnEngine() {
        var mockEngine = Substitute.For<IMacroEngine>();
        var service = new MacroExecutionService(mockEngine);
        var macro = new RoleplayMacro { Content = string.Empty };

        service.Execute(macro);

        mockEngine.Received(1).Play(macro);
    }

    [Fact]
    public void Execute_WhenMacroIsNull_DoesNothing() {
        var mockEngine = Substitute.For<IMacroEngine>();
        var service = new MacroExecutionService(mockEngine);

        service.Execute(null!);

        mockEngine.DidNotReceive().Play(Arg.Any<RoleplayMacro>());
    }
}