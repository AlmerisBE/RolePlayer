namespace RolePlayer.Tests.UI.Command.Services;

using Dalamud.Game.Command;
using Dalamud.Plugin.Services;
using NSubstitute;
using RolePlayer.UI.Command.Contracts;
using RolePlayer.UI.Command.Services;
using System.Collections.Generic;
using Xunit;

public class CommandDispatcherTests {
    [Fact]
    public void CommandDispatcher_OnInitialization_RegistersMainCommand() {
        var mockCommandManager = Substitute.For<ICommandManager>();
        var commands = new List<ICommand>();

        using var dispatcher = new CommandDispatcher(mockCommandManager, commands);

        mockCommandManager.Received(1).AddHandler("/roleplayer", Arg.Any<CommandInfo>());
    }

    [Fact]
    public void CommandDispatcher_OnCommand_DispatchesToCorrectCommandAction() {
        var mockCommandManager = Substitute.For<ICommandManager>();
        var mockCommand = Substitute.For<ICommand>();
        mockCommand.CommandTrigger.Returns("hello");

        var commands = new List<ICommand> { mockCommand };
        CommandInfo capturedCommandInfo = null!;

        mockCommandManager.When(x => x.AddHandler(Arg.Any<string>(), Arg.Any<CommandInfo>()))
            .Do(callInfo => capturedCommandInfo = callInfo.Arg<CommandInfo>());

        using var dispatcher = new CommandDispatcher(mockCommandManager, commands);

        capturedCommandInfo.Handler.Invoke("/roleplayer", "hello world");

        mockCommand.Received(1).Execute("world");
    }

    [Fact]
    public void CommandDispatcher_OnCommand_IsCaseInsensitive() {
        var mockCommandManager = Substitute.For<ICommandManager>();
        var mockCommand = Substitute.For<ICommand>();
        mockCommand.CommandTrigger.Returns("hello");

        var commands = new List<ICommand> { mockCommand };
        CommandInfo capturedCommandInfo = null!;

        mockCommandManager.When(x => x.AddHandler(Arg.Any<string>(), Arg.Any<CommandInfo>()))
            .Do(callInfo => capturedCommandInfo = callInfo.Arg<CommandInfo>());

        using var dispatcher = new CommandDispatcher(mockCommandManager, commands);

        capturedCommandInfo.Handler.Invoke("/roleplayer", "HeLlO arGuments");

        mockCommand.Received(1).Execute("arGuments");
    }

    [Fact]
    public void CommandDispatcher_OnDispose_RemovesCommandRegistration() {
        var mockCommandManager = Substitute.For<ICommandManager>();
        var commands = new List<ICommand>();

        var dispatcher = new CommandDispatcher(mockCommandManager, commands);
        dispatcher.Dispose();

        mockCommandManager.Received(1).RemoveHandler("/roleplayer");
    }
}