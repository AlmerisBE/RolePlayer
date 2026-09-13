namespace RolePlayer.Tests.Core.GameEngine.Services;

using Dalamud.Plugin;
using NSubstitute;
using RolePlayer.Core.GameEngine.Contracts;
using RolePlayer.Core.GameEngine.Models;
using RolePlayer.Core.GameEngine.Services;
using RolePlayer.Core.Logging.Contracts;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Xunit;

public class GameLibraryServiceTests : IDisposable {
    private string tempDirectory;
    private IDalamudPluginInterface mockPluginInterface;
    private ILoggerService mockLogger;
    private IGameTemplateProvider mockTemplateProvider;

    public GameLibraryServiceTests() {
        this.tempDirectory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(this.tempDirectory);

        this.mockPluginInterface = Substitute.For<IDalamudPluginInterface>();
        this.mockPluginInterface.ConfigDirectory.Returns(new DirectoryInfo(this.tempDirectory));

        this.mockLogger = Substitute.For<ILoggerService>();

        this.mockTemplateProvider = Substitute.For<IGameTemplateProvider>();
        this.mockTemplateProvider.GetDefaultTemplates().Returns(new Dictionary<string, GameDefinition>(StringComparer.OrdinalIgnoreCase));
    }

    [Fact]
    public void SaveGame_CreatesValidJsonFile() {
        var service = new GameLibraryService(this.mockPluginInterface, this.mockLogger, this.mockTemplateProvider);
        var game = new GameDefinition { Name = "Test Game" };

        service.SaveGame(game);

        string expectedPath = Path.Combine(service.LibraryDirectory, $"{game.Id}.json");
        Assert.True(File.Exists(expectedPath));
    }

    [Fact]
    public void DeleteGame_RemovesFile() {
        var service = new GameLibraryService(this.mockPluginInterface, this.mockLogger, this.mockTemplateProvider);
        var game = new GameDefinition { Name = "Game To Delete" };

        service.SaveGame(game);
        string expectedPath = Path.Combine(service.LibraryDirectory, $"{game.Id}.json");
        Assert.True(File.Exists(expectedPath));

        service.DeleteGame(game.Id);

        Assert.False(File.Exists(expectedPath));
    }

    [Fact]
    public void DuplicateGame_CreatesCopy() {
        var service = new GameLibraryService(this.mockPluginInterface, this.mockLogger, this.mockTemplateProvider);
        var game = new GameDefinition { Name = "Original Game" };

        service.SaveGame(game);
        service.DuplicateGame(game.Id);

        var games = service.GetAvailableGames().ToList();

        Assert.Contains(games, g => g.Name == "Original Game (Copy)");
        Assert.NotEqual(game.Id, games.First(g => g.Name == "Original Game (Copy)").Id);
    }

    public void Dispose() {
        if (Directory.Exists(this.tempDirectory)) Directory.Delete(this.tempDirectory, true);
    }
}