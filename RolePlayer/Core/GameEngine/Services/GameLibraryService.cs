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
            var deathRollGame = new GameDefinition {
                Name = "Death Roll",
                Author = "Almeris",
                Description = "A classic turn-based game of successive random rolls until someone rolls a 1.",
                AllowChatRegistration = false,
                InitialVariables = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase) {
                    { "current_max_roll", 999 },
                    { "current_player", "" }
                },
                Stages = new List<GameStage> {
                    new GameStage {
                        Id = "registration",
                        Name = "Registration",
                        GmDescription = "Wait for players to !join. Once ready, manually transition to 'In Progress'.",
                        OnEnterActions = new List<GameActionConfig> {
                            new GameActionConfig {
                                ActionType = "BroadcastMessage",
                                Parameters = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase) {
                                    { "Message", "Registration is open! Type !join to participate." }
                                }
                            }
                        },
                        ActiveModules = new List<GameModuleConfig> {
                            new GameModuleConfig {
                                ModuleType = "ChatListener",
                                Parameters = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase) {
                                    { "Command", "!join" }
                                },
                                OnTriggerActions = new List<GameActionConfig> {
                                    new GameActionConfig { ActionType = "RegisterPlayer" }
                                }
                            }
                        },
                        Transitions = new List<GameTransition> {
                            new GameTransition {
                                TargetStageId = "playing",
                                TriggerType = "Manual",
                                ConditionExpression = "Participants.Count >= 2"
                            }
                        }
                    },
                    new GameStage {
                        Id = "playing",
                        Name = "In Progress",
                        GmDescription = "Game is running. Engine tracks max roll and turns. First to roll 1 loses.",
                        OnEnterActions = new List<GameActionConfig> {
                            new GameActionConfig {
                                ActionType = "AdvanceTurn",
                                Parameters = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase) {
                                    { "TargetVar", "current_player" }
                                }
                            },
                            new GameActionConfig {
                                ActionType = "BroadcastMessage",
                                Parameters = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase) {
                                    { "Message", "[Start] Game begins! First to roll 1 loses. {Var.current_player}, you're up! (/random {Var.current_max_roll})" }
                                }
                            }
                        },
                        ActiveModules = new List<GameModuleConfig> {
                            new GameModuleConfig {
                                ModuleType = "DiceListener",
                                ConditionExpressions = new List<string> {
                                    "Participants CONTAINS Event.Sender",
                                    "Event.Sender != Var.current_player"
                                },
                                OnTriggerActions = new List<GameActionConfig> {
                                    new GameActionConfig {
                                        ActionType = "BroadcastMessage",
                                        Parameters = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase) {
                                            { "Message", "[!] Not your turn, {Event.Sender}! Waiting for {Var.current_player}." }
                                        }
                                    }
                                }
                            },
                            new GameModuleConfig {
                                ModuleType = "DiceListener",
                                ConditionExpressions = new List<string> {
                                    "Participants CONTAINS Event.Sender",
                                    "Event.Sender == Var.current_player",
                                    "Event.OutOf != Var.current_max_roll"
                                },
                                OnTriggerActions = new List<GameActionConfig> {
                                    new GameActionConfig {
                                        ActionType = "BroadcastMessage",
                                        Parameters = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase) {
                                            { "Message", "[!] Invalid roll, {Event.Sender}! Must roll out of {Var.current_max_roll}! (/random {Var.current_max_roll})" }
                                        }
                                    }
                                }
                            },
                            new GameModuleConfig {
                                ModuleType = "DiceListener",
                                ConditionExpressions = new List<string> {
                                    "Participants CONTAINS Event.Sender",
                                    "Event.Sender == Var.current_player",
                                    "Event.OutOf == Var.current_max_roll",
                                    "Event.Roll != 1"
                                },
                                OnTriggerActions = new List<GameActionConfig> {
                                    new GameActionConfig {
                                        ActionType = "SetVariable",
                                        Parameters = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase) {
                                            { "TargetVar", "current_max_roll" },
                                            { "Value", "{Event.Roll}" }
                                        }
                                    },
                                    new GameActionConfig {
                                        ActionType = "AdvanceTurn",
                                        Parameters = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase) {
                                            { "TargetVar", "current_player" }
                                        }
                                    },
                                    new GameActionConfig {
                                        ActionType = "BroadcastMessage",
                                        Parameters = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase) {
                                            { "Message", "[Roll] {Event.Sender} rolled {Event.Roll}. Next up: {Var.current_player} (/random {Var.current_max_roll})" }
                                        }
                                    }
                                }
                            },
                            new GameModuleConfig {
                                ModuleType = "DiceListener",
                                ConditionExpressions = new List<string> {
                                    "Participants CONTAINS Event.Sender",
                                    "Event.Sender == Var.current_player",
                                    "Event.OutOf == Var.current_max_roll",
                                    "Event.Roll == 1"
                                },
                                OnTriggerActions = new List<GameActionConfig> {
                                    new GameActionConfig {
                                        ActionType = "BroadcastMessage",
                                        Parameters = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase) {
                                            { "Message", "[Game Over] {Event.Sender} rolled a 1 and died!" }
                                        }
                                    },
                                    new GameActionConfig {
                                        ActionType = "StopGame"
                                    }
                                }
                            }
                        }
                    }
                }
            };
            this.WriteGameToFile(deathRollPath, deathRollGame);
        }

        var riddlesPath = Path.Combine(this.LibraryDirectory, "Riddles.json");
        if (!File.Exists(riddlesPath)) {
            var riddlesGame = new GameDefinition {
                Name = "Emote Riddles",
                Author = "Almeris",
                Description = "Answer the riddle by performing the correct emote. The GM sets the expected emote ID.",
                AllowChatRegistration = false,
                Parameters = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase) {
                    { "TrackScores", "true" }
                },
                InitialVariables = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase) {
                    { "expected_emote_id", 0 }
                },
                Stages = new List<GameStage> {
                    new GameStage {
                        Id = "registration",
                        Name = "Registration",
                        GmDescription = "Wait for players to !join. Once ready, manually transition to 'In Progress'.",
                        OnEnterActions = new List<GameActionConfig> {
                            new GameActionConfig {
                                ActionType = "BroadcastMessage",
                                Parameters = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase) {
                                    { "Message", "Registration for Emote Riddles is open! Type !join to participate." }
                                }
                            }
                        },
                        ActiveModules = new List<GameModuleConfig> {
                            new GameModuleConfig {
                                ModuleType = "ChatListener",
                                Parameters = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase) {
                                    { "Command", "!join" }
                                },
                                OnTriggerActions = new List<GameActionConfig> {
                                    new GameActionConfig { ActionType = "RegisterPlayer" }
                                }
                            }
                        },
                        Transitions = new List<GameTransition> {
                            new GameTransition {
                                TargetStageId = "playing",
                                TriggerType = "Manual",
                                ConditionExpression = "Participants.Count >= 1"
                            }
                        }
                    },
                    new GameStage {
                        Id = "playing",
                        Name = "In Progress",
                        GmDescription = "Set 'expected_emote_id' in variables, then ask your riddle in chat. First player to use the matching emote wins the round.",
                        OnEnterActions = new List<GameActionConfig> {
                            new GameActionConfig {
                                ActionType = "BroadcastMessage",
                                Parameters = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase) {
                                    { "Message", "[Start] The riddle game begins! Listen carefully to the GM and perform the correct emote to answer." }
                                }
                            }
                        },
                        ActiveModules = new List<GameModuleConfig> {
                            new GameModuleConfig {
                                ModuleType = "EmoteListener",
                                ConditionExpressions = new List<string> {
                                    "Participants CONTAINS Event.Sender",
                                    "Var.expected_emote_id != 0",
                                    "Event.EmoteId == Var.expected_emote_id"
                                },
                                OnTriggerActions = new List<GameActionConfig> {
                                    new GameActionConfig {
                                        ActionType = "BroadcastMessage",
                                        Parameters = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase) {
                                            { "Message", "[Winner] {Event.Sender} found the correct emote! Well done!" }
                                        }
                                    },
                                    new GameActionConfig {
                                        ActionType = "SetVariable",
                                        Parameters = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase) {
                                            { "TargetVar", "expected_emote_id" },
                                            { "Value", "0" }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            };
            this.WriteGameToFile(riddlesPath, riddlesGame);
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