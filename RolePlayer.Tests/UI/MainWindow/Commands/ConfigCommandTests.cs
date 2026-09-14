namespace RolePlayer.Tests.UI.MainWindow.Commands;

using NSubstitute;
using RolePlayer.UI.MainWindow.Commands;
using Xunit;

public class ConfigCommandTests {
    [Fact]
    public void Execute_CallsOpenConfigOnMainWindow() {
        var mockMainWindow = Substitute.ForPartsOf<RolePlayer.UI.MainWindow.Windows.MainWindow>(
            Substitute.For<Dalamud.Plugin.IDalamudPluginInterface>(),
            null!, null!, null!
        );

        var command = new ConfigCommand(mockMainWindow);

        command.Execute(string.Empty);

        mockMainWindow.Received(1).OpenConfig();
    }
}