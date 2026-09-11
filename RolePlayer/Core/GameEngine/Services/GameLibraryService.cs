namespace RolePlayer.Core.GameEngine.Services;

using Dalamud.Plugin;
using RolePlayer.Core.GameEngine.Contracts;
using RolePlayer.Core.GameEngine.Models;
using RolePlayer.Core.Logging.Contracts;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.Json;

public class GameLibraryService : IGameLibraryService {
    private IDalamudPluginInterface pluginInterface;
    private ILoggerService logger;

    private List<GameDefinition> cachedGames = new();
    private Dictionary<Guid, string> filePaths = new();

    public string LibraryDirectory => Path.Combine(this.pluginInterface.ConfigDirectory.FullName, "Games");

    public GameLibraryService(IDalamudPluginInterface pluginInterface, ILoggerService logger) {
        this.pluginInterface = pluginInterface;
        this.logger = logger;

        this.EnsureDirectoryAndDefaultGames();
        this.ReloadLibrary();
    }

    private void EnsureDirectoryAndDefaultGames() {
        if (!Directory.Exists(this.LibraryDirectory)) Directory.CreateDirectory(this.LibraryDirectory);

        var deathRollPath = Path.Combine(this.LibraryDirectory, "DeathRoll.json");
        if (!File.Exists(deathRollPath)) {
            var game = new GameDefinition {
                Name = "Death Roll",
                Author = "RolePlayer",
                Description = "A classic game of successive random rolls until someone rolls a 1.",
                Parameters = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase) {
                    { "StartingRoll", "999" },
                    { "DeathNumber", "1" }
                }
            };
            this.WriteGameToFile(deathRollPath, game);
        }

        var riddlesPath = Path.Combine(this.LibraryDirectory, "Riddles.json");
        if (!File.Exists(riddlesPath)) {
            var game = new GameDefinition {
                Name = "Emote Riddles",
                Author = "RolePlayer",
                Description = "Answer the riddle by performing the correct emote.",
                Parameters = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase) {
                    { "Riddle_1_Text", "I show joy without speaking. What am I?" },
                    { "Riddle_1_AnswerType", "Emote" },
                    { "Riddle_1_AnswerValue", "/joy" }
                }
            };
            this.WriteGameToFile(riddlesPath, game);
        }
    }

    private void WriteGameToFile(string path, GameDefinition game) {
        try {
            var options = new JsonSerializerOptions { WriteIndented = true };
            var json = JsonSerializer.Serialize(game, options);
            File.WriteAllText(path, json);
        }
        catch (Exception ex) {
            this.logger.Error(ex, $"Failed to write game definition to {path}.");
        }
    }

    public void ReloadLibrary() {
        this.cachedGames.Clear();
        this.filePaths.Clear();

        if (!Directory.Exists(this.LibraryDirectory)) return;

        var files = Directory.GetFiles(this.LibraryDirectory, "*.json");

        foreach (var file in files) {
            try {
                var json = File.ReadAllText(file);
                var game = JsonSerializer.Deserialize<GameDefinition>(json);
                if (game != null) {
                    this.cachedGames.Add(game);
                    this.filePaths[game.Id] = file;
                }
            }
            catch (Exception ex) {
                this.logger.Error(ex, $"Failed to parse game definition from {file}.");
            }
        }

        this.cachedGames = this.cachedGames.OrderBy(g => g.Name).ToList();
    }

    public IEnumerable<GameDefinition> GetAvailableGames() => this.cachedGames;

    public void SaveGame(GameDefinition game) {
        if (string.IsNullOrWhiteSpace(game.Name)) return;

        if (!this.filePaths.TryGetValue(game.Id, out string? filePath)) {
            // New game: use its Guid to guarantee a unique, collision-free filename
            filePath = Path.Combine(this.LibraryDirectory, $"{game.Id}.json");
            this.filePaths[game.Id] = filePath;
        }

        this.WriteGameToFile(filePath, game);
        this.ReloadLibrary();
    }

    public void DeleteGame(Guid gameId) {
        if (this.filePaths.TryGetValue(gameId, out string? filePath) && File.Exists(filePath)) {
            File.Delete(filePath);
            this.ReloadLibrary();
        }
    }

    public void DuplicateGame(Guid gameId) {
        var original = this.cachedGames.FirstOrDefault(g => g.Id == gameId);
        if (original == null) return;

        try {
            // Deep clone via JSON to break references securely
            var cloneJson = JsonSerializer.Serialize(original);
            var clone = JsonSerializer.Deserialize<GameDefinition>(cloneJson);

            if (clone != null) {
                clone.Id = Guid.NewGuid();
                clone.Name = $"{clone.Name} (Copy)";
                this.SaveGame(clone);
            }
        }
        catch (Exception ex) {
            this.logger.Error(ex, "Failed to duplicate game definition.");
        }
    }

    public void OpenLibraryDirectory() {
        if (!Directory.Exists(this.LibraryDirectory)) Directory.CreateDirectory(this.LibraryDirectory);

        try {
            Process.Start(new ProcessStartInfo {
                FileName = this.LibraryDirectory,
                UseShellExecute = true
            });
        }
        catch (Exception ex) {
            this.logger.Error(ex, "Failed to open games directory.");
        }
    }
}