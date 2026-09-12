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
                AllowChatRegistration = true,
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
                Description = "Answer the riddle by performing the correct emote before time runs out! First to reach the max score wins.",
                AllowChatRegistration = true,
                Parameters = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase) {
                    { "TrackScores", "true" }
                },
                InitialVariables = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase) {
                    { "expected_emote_id", 0 },
                    { "riddle_text", "I am joy incarnate!" },
                    { "round_duration", 30 },
                    { "max_score", 3 }
                },
                ExposedVariables = new List<GameVariableDefinition> {
                    new GameVariableDefinition { Key = "expected_emote_id", Label = "Expected Emote (Answer)", Type = "Emote" },
                    new GameVariableDefinition { Key = "riddle_text", Label = "Riddle Text", Type = "String" },
                    new GameVariableDefinition { Key = "round_duration", Label = "Round Duration (Seconds)", Type = "Number" },
                    new GameVariableDefinition { Key = "max_score", Label = "Score to Win", Type = "Number" }
                },
                Stages = new List<GameStage> {
                    new GameStage {
                        Id = "registration",
                        Name = "Registration & Preparation",
                        GmDescription = "Wait for players to !join. Configure your riddle in the UI above, then manually transition.",
                        OnEnterActions = new List<GameActionConfig> {
                            new GameActionConfig {
                                ActionType = "BroadcastMessage",
                                Parameters = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase) { { "Message", "Registration for Emote Riddles is open! Type !join to participate." } }
                            }
                        },
                        ActiveModules = new List<GameModuleConfig> {
                            new GameModuleConfig {
                                ModuleType = "ChatListener",
                                Parameters = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase) { { "Command", "!join" } },
                                OnTriggerActions = new List<GameActionConfig> { new GameActionConfig { ActionType = "RegisterPlayer" } }
                            }
                        },
                        Transitions = new List<GameTransition> {
                            new GameTransition { TargetStageId = "countdown", TriggerType = "Manual", ConditionExpression = "Participants.Count >= 1" }
                        }
                    },

                    new GameStage {
                        Id = "countdown",
                        Name = "Countdown",
                        GmDescription = "Broadcasts the riddle with a countdown. Automatically advances to Action Phase after 4 seconds.",
                        OnEnterActions = new List<GameActionConfig> {
                            new GameActionConfig {
                                ActionType = "BroadcastMessage",
                                Parameters = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase) {
                                    { "Message", "The riddle begins in 3...\\n2...\\n1...\\n[Riddle] {Var.riddle_text}" }
                                }
                            }
                        },
                        ActiveModules = new List<GameModuleConfig> {
                            new GameModuleConfig {
                                ModuleType = "TimerListener",
                                Parameters = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase) { { "DurationSeconds", "5" } },
                                OnTriggerActions = new List<GameActionConfig> { new GameActionConfig { ActionType = "AdvanceStage" } }
                            }
                        },
                        Transitions = new List<GameTransition> {
                            new GameTransition { TargetStageId = "playing", TriggerType = "Manual" }
                        }
                    },

                    new GameStage {
                        Id = "playing",
                        Name = "Action Phase",
                        GmDescription = "Engine listens for the correct emote and tracks time.",
                        OnEnterActions = new List<GameActionConfig> {
                            new GameActionConfig {
                                ActionType = "BroadcastMessage",
                                Parameters = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase) {
                                    { "Message", "[Start] You have {Var.round_duration} seconds to perform the emote!" }
                                }
                            }
                        },
                        ActiveModules = new List<GameModuleConfig> {
                            new GameModuleConfig {
                                ModuleType = "ChatListener",
                                Parameters = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase) { { "Command", "!score" } },
                                OnTriggerActions = new List<GameActionConfig> {
                                    new GameActionConfig { ActionType = "BroadcastScores", Parameters = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase) { { "Prefix", "score_" } } }
                                }
                            },
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
                                        Parameters = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase) { { "Message", "[Winner] {Event.Sender} found the correct emote! Well done!" } }
                                    },
                                    new GameActionConfig {
                                        ActionType = "IncrementVariable",
                                        Parameters = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase) { { "TargetVar", "score_{Event.Sender}" }, { "Value", "1" } }
                                    },
                                    new GameActionConfig {
                                        ActionType = "EndGameIfScoreReached",
                                        Parameters = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase) { { "Prefix", "score_" }, { "TargetScore", "{Var.max_score}" } }
                                    },
                                    new GameActionConfig {
                                        ActionType = "SetVariable",
                                        Parameters = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase) { { "TargetVar", "expected_emote_id" }, { "Value", "0" } }
                                    },
                                    new GameActionConfig { ActionType = "AdvanceStage" }
                                }
                            },
                            new GameModuleConfig {
                                ModuleType = "TimerListener",
                                Parameters = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase) { { "DurationSeconds", "{Var.round_duration}" } },
                                OnTriggerActions = new List<GameActionConfig> {
                                    new GameActionConfig {
                                        ActionType = "BroadcastMessage",
                                        Parameters = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase) { { "Message", "[Time's Up] No one found the answer in time!" } }
                                    },
                                    new GameActionConfig { ActionType = "AdvanceStage" }
                                }
                            }
                        },
                        Transitions = new List<GameTransition> {
                            new GameTransition { TargetStageId = "round_end", TriggerType = "Manual" }
                        }
                    },
                    new GameStage {
                        Id = "round_end",
                        Name = "Round Ended",
                        GmDescription = "Prepare the next riddle and return to Countdown, or Stop the Session.",
                        ActiveModules = new List<GameModuleConfig> {
                            new GameModuleConfig {
                                ModuleType = "ChatListener",
                                Parameters = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase) { { "Command", "!score" } },
                                OnTriggerActions = new List<GameActionConfig> {
                                    new GameActionConfig { ActionType = "BroadcastScores", Parameters = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase) { { "Prefix", "score_" } } }
                                }
                            }
                        },
                        Transitions = new List<GameTransition> {
                            // CORRECTION : La boucle repart au compte à rebours, pas directement à la phase d'action
                            new GameTransition { TargetStageId = "countdown", TriggerType = "Manual" }
                        }
                    }
                }
            };
            this.WriteGameToFile(riddlesPath, riddlesGame);
        }

        var truthOrDarePath = Path.Combine(this.LibraryDirectory, "TruthOrDare.json");
        if (!File.Exists(truthOrDarePath)) {
            var truthGame = new GameDefinition {
                Name = "Truth or Dare",
                Author = "Almeris",
                Description = "Everyone rolls. Highest score asks Truth or Dare to the lowest score.",
                AllowChatRegistration = true,
                InitialVariables = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase) {
                    { "highest_roll", 0 }, { "lowest_roll", 999 },
                    { "highest_player", "" }, { "lowest_player", "" },
                    { "rolls_count", 0 }
                },
                Stages = new List<GameStage> {
                    new GameStage {
                        Id = "registration",
                        Name = "Registration",
                        GmDescription = "Wait for players to !join.",
                        ActiveModules = new List<GameModuleConfig> {
                            new GameModuleConfig {
                                ModuleType = "ChatListener", Parameters = new Dictionary<string, string> { { "Command", "!join" } },
                                OnTriggerActions = new List<GameActionConfig> { new GameActionConfig { ActionType = "RegisterPlayer" } }
                            }
                        },
                        Transitions = new List<GameTransition> { new GameTransition { TargetStageId = "rolling", TriggerType = "Manual", ConditionExpression = "Participants.Count >= 2" } }
                    },
                    new GameStage {
                        Id = "rolling",
                        Name = "Rolling Phase",
                        GmDescription = "Everyone rolls. The engine tracks the highest and lowest.",
                        OnEnterActions = new List<GameActionConfig> {
                            new GameActionConfig { ActionType = "SetVariable", Parameters = new Dictionary<string, string> { { "TargetVar", "highest_roll" }, { "Value", "0" } } },
                            new GameActionConfig { ActionType = "SetVariable", Parameters = new Dictionary<string, string> { { "TargetVar", "lowest_roll" }, { "Value", "999" } } },
                            new GameActionConfig { ActionType = "SetVariable", Parameters = new Dictionary<string, string> { { "TargetVar", "rolls_count" }, { "Value", "0" } } },
                            new GameActionConfig { ActionType = "BroadcastMessage", Parameters = new Dictionary<string, string> { { "Message", "Time to roll! Everyone use /random 100!" } } }
                        },
                        ActiveModules = new List<GameModuleConfig> {
                            new GameModuleConfig {
                                ModuleType = "DiceListener", ConditionExpressions = new List<string> { "Participants CONTAINS Event.Sender", "Event.Roll > Var.highest_roll" },
                                OnTriggerActions = new List<GameActionConfig> {
                                    new GameActionConfig { ActionType = "SetVariable", Parameters = new Dictionary<string, string> { { "TargetVar", "highest_roll" }, { "Value", "{Event.Roll}" } } },
                                    new GameActionConfig { ActionType = "SetVariable", Parameters = new Dictionary<string, string> { { "TargetVar", "highest_player" }, { "Value", "{Event.Sender}" } } }
                                }
                            },
                            new GameModuleConfig {
                                ModuleType = "DiceListener", ConditionExpressions = new List<string> { "Participants CONTAINS Event.Sender", "Event.Roll < Var.lowest_roll" },
                                OnTriggerActions = new List<GameActionConfig> {
                                    new GameActionConfig { ActionType = "SetVariable", Parameters = new Dictionary<string, string> { { "TargetVar", "lowest_roll" }, { "Value", "{Event.Roll}" } } },
                                    new GameActionConfig { ActionType = "SetVariable", Parameters = new Dictionary<string, string> { { "TargetVar", "lowest_player" }, { "Value", "{Event.Sender}" } } }
                                }
                            },
                            new GameModuleConfig {
                                ModuleType = "DiceListener", ConditionExpressions = new List<string> { "Participants CONTAINS Event.Sender" },
                                OnTriggerActions = new List<GameActionConfig> { new GameActionConfig { ActionType = "IncrementVariable", Parameters = new Dictionary<string, string> { { "TargetVar", "rolls_count" }, { "Value", "1" } } } }
                            }
                        },
                        Transitions = new List<GameTransition> { new GameTransition { TargetStageId = "resolution", TriggerType = "OnEvent", ConditionExpression = "Var.rolls_count == Participants.Count" } }
                    },
                    new GameStage {
                        Id = "resolution",
                        Name = "Resolution",
                        GmDescription = "Results are announced.",
                        OnEnterActions = new List<GameActionConfig> {
                            new GameActionConfig { ActionType = "BroadcastMessage", Parameters = new Dictionary<string, string> { { "Message", "Results: {Var.highest_player} ({Var.highest_roll}) VS {Var.lowest_player} ({Var.lowest_roll})! {Var.highest_player}, what is your Truth or Dare?" } } }
                        },
                        Transitions = new List<GameTransition> { new GameTransition { TargetStageId = "rolling", TriggerType = "Manual" } }
                    }
                }
            };
            this.WriteGameToFile(truthOrDarePath, truthGame);
        }

        var blackjackPath = Path.Combine(this.LibraryDirectory, "Blackjack.json");
        if (!File.Exists(blackjackPath)) {
            var blackjackGame = new GameDefinition {
                Name = "Dice Blackjack",
                Author = "Almeris",
                Description = "Roll closer to 21 without busting. Type !hit to roll or !stand to pass.",
                AllowChatRegistration = true,
                Parameters = new Dictionary<string, string> { { "TrackScores", "true" } },
                InitialVariables = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase) { { "current_player", "" }, { "played_count", 0 } },
                Stages = new List<GameStage> {
                    new GameStage {
                        Id = "registration", Name = "Registration", GmDescription = "Wait for players.",
                        ActiveModules = new List<GameModuleConfig> {
                            new GameModuleConfig { ModuleType = "ChatListener", Parameters = new Dictionary<string, string> { { "Command", "!join" } }, OnTriggerActions = new List<GameActionConfig> { new GameActionConfig { ActionType = "RegisterPlayer" } } }
                        },
                        Transitions = new List<GameTransition> { new GameTransition { TargetStageId = "playing", TriggerType = "Manual", ConditionExpression = "Participants.Count >= 1" } }
                    },
                    new GameStage {
                        Id = "playing", Name = "Player Turn", GmDescription = "Current player decides to hit or stand.",
                        OnEnterActions = new List<GameActionConfig> {
                            new GameActionConfig { ActionType = "AdvanceTurn", Parameters = new Dictionary<string, string> { { "TargetVar", "current_player" } } },
                            new GameActionConfig { ActionType = "BroadcastMessage", Parameters = new Dictionary<string, string> { { "Message", "{Var.current_player}'s turn! Type !hit to roll (/random 10) or !stand." } } }
                        },
                        ActiveModules = new List<GameModuleConfig> {
                            new GameModuleConfig {
                                ModuleType = "DiceListener", ConditionExpressions = new List<string> { "Event.Sender == Var.current_player" },
                                OnTriggerActions = new List<GameActionConfig> {
                                    new GameActionConfig { ActionType = "IncrementVariable", Parameters = new Dictionary<string, string> { { "TargetVar", "score_{Event.Sender}" }, { "Value", "{Event.Roll}" } } },
                                    new GameActionConfig { ActionType = "BroadcastMessage", Parameters = new Dictionary<string, string> { { "Message", "{Event.Sender} rolled {Event.Roll}. Total: {Var.score_{Event.Sender}}" } } }
                                }
                            },
                            new GameModuleConfig {
                                ModuleType = "ChatListener", Parameters = new Dictionary<string, string> { { "Command", "!stand" } }, ConditionExpressions = new List<string> { "Event.Sender == Var.current_player" },
                                OnTriggerActions = new List<GameActionConfig> {
                                    new GameActionConfig { ActionType = "BroadcastMessage", Parameters = new Dictionary<string, string> { { "Message", "{Event.Sender} stands." } } },
                                    new GameActionConfig { ActionType = "AdvanceStage" }
                                }
                            }
                        },
                        Transitions = new List<GameTransition> {
                            new GameTransition { TargetStageId = "next_turn", TriggerType = "OnEvent", ConditionExpression = "Var.score_{Event.Sender} >= 21" },
                            new GameTransition { TargetStageId = "next_turn", TriggerType = "Manual" }
                        }
                    },
                    new GameStage {
                        Id = "next_turn", Name = "Turn Resolution", GmDescription = "Checking if all players have played.",
                        OnEnterActions = new List<GameActionConfig> {
                            new GameActionConfig { ActionType = "IncrementVariable", Parameters = new Dictionary<string, string> { { "TargetVar", "played_count" }, { "Value", "1" } } }
                        },
                        Transitions = new List<GameTransition> {
                            new GameTransition { TargetStageId = "resolution", TriggerType = "Auto", ConditionExpression = "Var.played_count == Participants.Count" },
                            new GameTransition { TargetStageId = "playing", TriggerType = "Auto", ConditionExpression = "Var.played_count != Participants.Count" }
                        }
                    },
                    new GameStage {
                        Id = "resolution", Name = "Results", GmDescription = "Game over.",
                        OnEnterActions = new List<GameActionConfig> {
                            new GameActionConfig { ActionType = "BroadcastScores", Parameters = new Dictionary<string, string> { { "Prefix", "score_" } } },
                            new GameActionConfig { ActionType = "BroadcastMessage", Parameters = new Dictionary<string, string> { { "Message", "Game over! The closest to 21 without busting wins!" } } }
                        }
                    }
                }
            };
            this.WriteGameToFile(blackjackPath, blackjackGame);
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