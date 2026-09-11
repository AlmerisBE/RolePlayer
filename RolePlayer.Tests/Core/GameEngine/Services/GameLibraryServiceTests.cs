namespace RolePlayer.Tests.Core.GameEngine.Services;

using Dalamud.Plugin;
using NSubstitute;
using RolePlayer.Core.GameEngine.Models;
using RolePlayer.Core.GameEngine.Services;
using RolePlayer.Core.Logging.Contracts;
using System;
using System.IO;
using System.Linq;
using Xunit;

public class GameLibraryServiceTests : IDisposable {
    private string tempDirectory;
    private IDalamudPluginInterface mockPluginInterface;
    private ILoggerService mockLogger;

    public GameLibraryServiceTests() {
        this.tempDirectory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(this.tempDirectory);

        this.mockPluginInterface = Substitute.For<IDalamudPluginInterface>();
        this.mockPluginInterface.ConfigDirectory.Returns(new DirectoryInfo(this.tempDirectory));

        this.mockLogger = Substitute.For<ILoggerService>();
    }

    [Fact]
    public void SaveGame_CreatesValidJsonFile() {
        var service = new GameLibraryService(this.mockPluginInterface, this.mockLogger);
        var game = new GameDefinition { Name = "Test Game" };

        service.SaveGame(game);

        // We now assert against the game.Id instead of the sanitized game.Name
        string expectedPath = Path.Combine(service.LibraryDirectory, $"{game.Id}.json");
        Assert.True(File.Exists(expectedPath));
    }

    [Fact]
    public void DeleteGame_RemovesFile() {
        var service = new GameLibraryService(this.mockPluginInterface, this.mockLogger);
        var game = new GameDefinition { Name = "Game To Delete" };

        service.SaveGame(game);
        string expectedPath = Path.Combine(service.LibraryDirectory, $"{game.Id}.json");
        Assert.True(File.Exists(expectedPath));

        service.DeleteGame(game.Id);

        Assert.False(File.Exists(expectedPath));
    }

    [Fact]
    public void DuplicateGame_CreatesCopy() {
        var service = new GameLibraryService(this.mockPluginInterface, this.mockLogger);
        var game = new GameDefinition { Name = "Original Game" };

        service.SaveGame(game);
        service.DuplicateGame(game.Id);

        var games = service.GetAvailableGames().ToList();

        // We expect the default games (Death Roll, Riddles) + Original + Copy
        Assert.Contains(games, g => g.Name == "Original Game (Copy)");
        Assert.NotEqual(game.Id, games.First(g => g.Name == "Original Game (Copy)").Id);
    }

    public void Dispose() {
        if (Directory.Exists(this.tempDirectory)) {
            Directory.Delete(this.tempDirectory, true);
        }
    }
}