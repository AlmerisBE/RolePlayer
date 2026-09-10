namespace RolePlayer.Tests.Core.Macros.Services;

using NSubstitute;
using RolePlayer.API.Interop.Contracts;
using RolePlayer.Core.Macros.Models;
using RolePlayer.Core.Macros.Services;
using Xunit;

public class MacroExecutionServiceTests {
    [Fact]
    public void Execute_WhenMacroIsValid_CallsNativeExecution() {
        var mockNativeExecution = Substitute.For<INativeExecutionService>();
        var service = new MacroExecutionService(mockNativeExecution);
        var macro = new RoleplayMacro { Content = "/bow" };

        service.Execute(macro);

        mockNativeExecution.Received(1).Execute("/bow");
    }

    [Fact]
    public void Execute_WhenMacroContentIsEmpty_DoesNothing() {
        var mockNativeExecution = Substitute.For<INativeExecutionService>();
        var service = new MacroExecutionService(mockNativeExecution);
        var macro = new RoleplayMacro { Content = string.Empty };

        service.Execute(macro);

        mockNativeExecution.DidNotReceive().Execute(Arg.Any<string>());
    }

    [Fact]
    public void Execute_WhenMacroIsNull_DoesNothing() {
        var mockNativeExecution = Substitute.For<INativeExecutionService>();
        var service = new MacroExecutionService(mockNativeExecution);

        service.Execute(null!);

        mockNativeExecution.DidNotReceive().Execute(Arg.Any<string>());
    }
}