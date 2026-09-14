namespace RolePlayer.Tests.UI.MainWindow.Commands;

using NSubstitute;
using RolePlayer.UI.MainWindow.Commands;
using RolePlayer.UI.MainWindow.Contracts;
using Xunit;

public class ConfigCommandTests {
    [Fact]
    public void Execute_CallsOpenConfigOnMainWindow() {
        var mockMainWindow = Substitute.For<IMainWindow>();
        var command = new ConfigCommand(mockMainWindow);

        command.Execute(string.Empty);

        mockMainWindow.Received(1).OpenConfig();
    }
}