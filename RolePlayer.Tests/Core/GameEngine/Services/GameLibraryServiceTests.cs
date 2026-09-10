namespace RolePlayer.Tests.Core.GameEngine.Services;

using Dalamud.Plugin;
using NSubstitute;
using RolePlayer.Core.GameEngine.Models;
using RolePlayer.Core.GameEngine.Services;
using RolePlayer.Core.Logging.Contracts;
using System.IO;
using System.Linq;
using Xunit;

public class GameLibraryServiceTests {
    [Fact]
    public void Constructor_CreatesLibraryDirectoryAndDefaultGames_WhenMissing() {
        var mockPluginInterface = Substitute.For<IDalamudPluginInterface>();
        var mockLogger = Substitute.For<ILoggerService>();

        var tempPath = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        mockPluginInterface.ConfigDirectory.Returns(new DirectoryInfo(tempPath));

        try {
            var service = new GameLibraryService(mockPluginInterface, mockLogger);

            Assert.True(Directory.Exists(service.LibraryDirectory));
            Assert.True(File.Exists(Path.Combine(service.LibraryDirectory, "DeathRoll.json")));
            Assert.True(File.Exists(Path.Combine(service.LibraryDirectory, "Riddles.json")));
        }
        finally {
            if (Directory.Exists(tempPath)) Directory.Delete(tempPath, true);
        }
    }

    [Fact]
    public void SaveGame_CreatesValidJsonFile() {
        var mockPluginInterface = Substitute.For<IDalamudPluginInterface>();
        var mockLogger = Substitute.For<ILoggerService>();

        var tempPath = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        mockPluginInterface.ConfigDirectory.Returns(new DirectoryInfo(tempPath));

        try {
            var service = new GameLibraryService(mockPluginInterface, mockLogger);
            var newGame = new GameDefinition {
                Name = "TestGame",
                Author = "Tester"
            };

            service.SaveGame(newGame);

            var filePath = Path.Combine(service.LibraryDirectory, "TestGame.json");
            Assert.True(File.Exists(filePath));

            var content = File.ReadAllText(filePath);
            Assert.Contains("TestGame", content);
            Assert.Contains("Tester", content);
        }
        finally {
            if (Directory.Exists(tempPath)) Directory.Delete(tempPath, true);
        }
    }

    [Fact]
    public void GetAvailableGames_ParsesJsonFilesSuccessfully() {
        var mockPluginInterface = Substitute.For<IDalamudPluginInterface>();
        var mockLogger = Substitute.For<ILoggerService>();

        var tempPath = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        mockPluginInterface.ConfigDirectory.Returns(new DirectoryInfo(tempPath));

        try {
            var service = new GameLibraryService(mockPluginInterface, mockLogger);

            var games = service.GetAvailableGames().ToList();

            Assert.NotEmpty(games);
            Assert.Contains(games, g => g.Name == "Death Roll");
            Assert.Contains(games, g => g.Name == "Emote Riddles");
        }
        finally {
            if (Directory.Exists(tempPath)) Directory.Delete(tempPath, true);
        }
    }
}