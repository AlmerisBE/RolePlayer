namespace RolePlayer.Tests.UI.Themes.Services;

using Dalamud.Plugin;
using NSubstitute;
using RolePlayer.Core.Configuration.Contracts;
using RolePlayer.Core.Configuration.Models;
using RolePlayer.Core.Logging.Contracts;
using RolePlayer.UI.Themes.Services;
using System.IO;
using System.Linq;
using Xunit;

public class ThemeManagementServiceTests {
    [Fact]
    public void Constructor_CreatesThemeDirectoryAndDefaultFiles_WhenTheyDoNotExist() {
        var mockPluginInterface = Substitute.For<IDalamudPluginInterface>();
        var mockConfigService = Substitute.For<IConfigurationService>();
        var mockLogger = Substitute.For<ILoggerService>();

        var tempPath = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        mockPluginInterface.ConfigDirectory.Returns(new DirectoryInfo(tempPath));
        mockConfigService.GetConfig().Returns(new PluginConfiguration { SelectedTheme = "Default" });

        try {
            var service = new ThemeManagementService(mockPluginInterface, mockConfigService, mockLogger);

            Assert.True(Directory.Exists(service.ThemeDirectory));
            Assert.True(File.Exists(Path.Combine(service.ThemeDirectory, "Dark.json")));
            Assert.True(File.Exists(Path.Combine(service.ThemeDirectory, "Light.json")));
        }
        finally {
            if (Directory.Exists(tempPath)) Directory.Delete(tempPath, true);
        }
    }

    [Fact]
    public void GetAvailableThemes_ReturnsOnlyJsonFileNames() {
        var mockPluginInterface = Substitute.For<IDalamudPluginInterface>();
        var mockConfigService = Substitute.For<IConfigurationService>();
        var mockLogger = Substitute.For<ILoggerService>();

        var tempPath = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        mockPluginInterface.ConfigDirectory.Returns(new DirectoryInfo(tempPath));
        mockConfigService.GetConfig().Returns(new PluginConfiguration { SelectedTheme = "Default" });

        try {
            var service = new ThemeManagementService(mockPluginInterface, mockConfigService, mockLogger);

            var themes = service.GetAvailableThemes().ToList();

            Assert.Contains("Dark", themes);
            Assert.Contains("Light", themes);
            Assert.DoesNotContain("Dark.json", themes);
        }
        finally {
            if (Directory.Exists(tempPath)) Directory.Delete(tempPath, true);
        }
    }
}