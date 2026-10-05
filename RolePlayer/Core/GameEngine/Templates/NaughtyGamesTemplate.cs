namespace RolePlayer.Core.GameEngine.Templates;

using RolePlayer.Core.GameEngine.Contracts;
using RolePlayer.Core.GameEngine.Models;
using System;
using System.Collections.Generic;

public class NaughtyGamesTemplate : IDefaultGameTemplate {
    public string FileName => "NaughtyGames.json";

    public GameDefinition Build() {
        return new GameDefinition {
            Name = "Naughty Games Box",
            Author = "Almeris",
            Description = "An endless listening loop for social games. Available commands: !actions, !tod, !naked, !emote. Supports smart tie-breakers.",
            AllowChatRegistration = true,
            InitialVariables = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase) {
                { "next_game", "" }
            },
            Stages = new List<GameStage> {
                new GameStage {
                    Id = "startup",
                    Name = "Startup Initialization",
                    GmDescription = "Broadcasts the welcome message and automatically moves to the idle listening loop.",
                    OnEnterActions = new List<GameActionConfig> {
                        new GameActionConfig {
                            ActionType = "BroadcastMessage",
                            Parameters = new Dictionary<string, string> {
                                { "Message", "[Naughty Games] The game session begins! Use !tod, !naked, or !emote to play (!actions for the command list)." }
                            }
                        }
                    },
                    Transitions = new List<GameTransition> {
                        new GameTransition { TargetStageId = "idle", TriggerType = "Auto" }
                    }
                },
                new GameStage {
                    Id = "idle",
                    Name = "Listening Loop",
                    GmDescription = "Waiting for chat commands (!actions, !tod, !naked, !emote).",
                    ActiveModules = new List<GameModuleConfig> {
                        new GameModuleConfig {
                            ModuleType = "ChatListener", Parameters = new Dictionary<string, string> { { "Command", "!actions" } },
                            OnTriggerActions = new List<GameActionConfig> {
                                new GameActionConfig { ActionType = "BroadcastMessage", Parameters = new Dictionary<string, string> { { "Message", "[Naughty Games] Available commands: !tod (Truth or Dare), !naked (Strip if <= 250), !emote (Sexy emote if > 800)" } } }
                            }
                        },
                        new GameModuleConfig {
                            ModuleType = "ChatListener", Parameters = new Dictionary<string, string> { { "Command", "!tod" } },
                            OnTriggerActions = new List<GameActionConfig> { new GameActionConfig { ActionType = "SetVariable", Parameters = new Dictionary<string, string> { { "TargetVar", "next_game" }, { "Value", "tod" } } } }
                        },
                        new GameModuleConfig {
                            ModuleType = "ChatListener", Parameters = new Dictionary<string, string> { { "Command", "!naked" } },
                            OnTriggerActions = new List<GameActionConfig> { new GameActionConfig { ActionType = "SetVariable", Parameters = new Dictionary<string, string> { { "TargetVar", "next_game" }, { "Value", "naked" } } } }
                        },
                        new GameModuleConfig {
                            ModuleType = "ChatListener", Parameters = new Dictionary<string, string> { { "Command", "!emote" } },
                            OnTriggerActions = new List<GameActionConfig> { new GameActionConfig { ActionType = "SetVariable", Parameters = new Dictionary<string, string> { { "TargetVar", "next_game" }, { "Value", "emote" } } } }
                        }
                    },
                    Transitions = new List<GameTransition> {
                        new GameTransition { TargetStageId = "tod_start", TriggerType = "OnEvent", ConditionExpression = "Var.next_game == 'tod'" },
                        new GameTransition { TargetStageId = "naked_start", TriggerType = "OnEvent", ConditionExpression = "Var.next_game == 'naked'" },
                        new GameTransition { TargetStageId = "emote_start", TriggerType = "OnEvent", ConditionExpression = "Var.next_game == 'emote'" }
                    }
                },
                new GameStage {
                    Id = "tod_start", Name = "TOD: Initialization", GmDescription = "Starts the Truth or Dare setup.",
                    OnEnterActions = new List<GameActionConfig> {
                        new GameActionConfig { ActionType = "ClearVariables", Parameters = new Dictionary<string, string> { { "Prefix", "tod_score_" } } },
                        new GameActionConfig { ActionType = "SetVariable", Parameters = new Dictionary<string, string> { { "TargetVar", "next_game" }, { "Value", "" } } },
                        new GameActionConfig { ActionType = "SetVariable", Parameters = new Dictionary<string, string> { { "TargetVar", "tod_resolving" }, { "Value", "normal" } } },
                        new GameActionConfig { ActionType = "BroadcastMessage", Parameters = new Dictionary<string, string> { { "Message", "[Truth or Dare] Starting! Roll your /random to participate. The highest score will impose a truth or dare to the lowest score. You have 60 seconds!" } } }
                    },
                    Transitions = new List<GameTransition> { new GameTransition { TargetStageId = "tod_listen", TriggerType = "Auto" } }
                },
                new GameStage {
                    Id = "tod_listen", Name = "TOD: Listening", GmDescription = "Listening for /random rolls for 60 seconds.",
                    ActiveModules = new List<GameModuleConfig> {
                        new GameModuleConfig {
                            ModuleType = "DiceListener", ConditionExpressions = new List<string> { "Var.tod_score_{Event.Sender} == null" },
                            OnTriggerActions = new List<GameActionConfig> { new GameActionConfig { ActionType = "SetVariable", Parameters = new Dictionary<string, string> { { "TargetVar", "tod_score_{Event.Sender}" }, { "Value", "{Event.Roll}" } } } }
                        },
                        new GameModuleConfig {
                            ModuleType = "DiceListener", ConditionExpressions = new List<string> { "Var.tod_score_{Event.Sender} != null" },
                            OnTriggerActions = new List<GameActionConfig> { new GameActionConfig { ActionType = "BroadcastMessage", Parameters = new Dictionary<string, string> { { "Message", "[!] You can only roll once per round, {Event.Sender}!" } } } }
                        },
                        new GameModuleConfig {
                            ModuleType = "TimerListener",
                            Parameters = new Dictionary<string, string> { { "DurationSeconds", "60" } },
                            OnTriggerActions = new List<GameActionConfig> { new GameActionConfig { ActionType = "AdvanceStage" } }
                        }
                    },
                    Transitions = new List<GameTransition> { new GameTransition { TargetStageId = "tod_eval", TriggerType = "Manual" } }
                },
                new GameStage {
                    Id = "tod_eval", Name = "TOD: Evaluation", GmDescription = "Evaluates TOD scores and handles ties.",
                    OnEnterActions = new List<GameActionConfig> {
                        new GameActionConfig { ActionType = "EvaluateMinMax", Parameters = new Dictionary<string, string> { { "Prefix", "tod_score_" }, { "FilterVar", "tod_expected_players" } } },
                        new GameActionConfig { ActionType = "EvaluateEquality" }
                    },
                    Transitions = new List<GameTransition> {
                        new GameTransition { TargetStageId = "idle", TriggerType = "Auto", ConditionExpression = "Var.tod_resolving == 'done'" },
                        new GameTransition { TargetStageId = "tod_tie_start", TriggerType = "Auto", ConditionExpression = "Var.tod_resolving != 'done'" }
                    }
                },
                new GameStage {
                    Id = "tod_tie_start", Name = "TOD: Tie Initialization", GmDescription = "Clears previous scores to prepare for a tie breaker.",
                    OnEnterActions = new List<GameActionConfig> {
                        new GameActionConfig { ActionType = "ClearVariables", Parameters = new Dictionary<string, string> { { "Prefix", "tod_score_" } } }
                    },
                    Transitions = new List<GameTransition> { new GameTransition { TargetStageId = "tod_listen", TriggerType = "Auto" } }
                },
                new GameStage {
                    Id = "naked_start", Name = "Naked: Initialization", GmDescription = "Starts the Naked game setup.",
                    OnEnterActions = new List<GameActionConfig> {
                        new GameActionConfig { ActionType = "ClearVariables", Parameters = new Dictionary<string, string> { { "Prefix", "naked_score_" } } },
                        new GameActionConfig { ActionType = "SetVariable", Parameters = new Dictionary<string, string> { { "TargetVar", "next_game" }, { "Value", "" } } },
                        new GameActionConfig { ActionType = "BroadcastMessage", Parameters = new Dictionary<string, string> { { "Message", "[Naked Game] Roll your /random! If you score 250 or less, you must strip for 15 minutes! You have 60 seconds." } } }
                    },
                    Transitions = new List<GameTransition> { new GameTransition { TargetStageId = "naked_listen", TriggerType = "Auto" } }
                },
                new GameStage {
                    Id = "naked_listen", Name = "Naked: Listening", GmDescription = "Listening for /random rolls for 60 seconds.",
                    ActiveModules = new List<GameModuleConfig> {
                        new GameModuleConfig {
                            ModuleType = "DiceListener", ConditionExpressions = new List<string> { "Var.naked_score_{Event.Sender} == null" },
                            OnTriggerActions = new List<GameActionConfig> { new GameActionConfig { ActionType = "SetVariable", Parameters = new Dictionary<string, string> { { "TargetVar", "naked_score_{Event.Sender}" }, { "Value", "{Event.Roll}" } } } }
                        },
                        new GameModuleConfig {
                            ModuleType = "DiceListener", ConditionExpressions = new List<string> { "Var.naked_score_{Event.Sender} != null" },
                            OnTriggerActions = new List<GameActionConfig> { new GameActionConfig { ActionType = "BroadcastMessage", Parameters = new Dictionary<string, string> { { "Message", "[!] You can only roll once, {Event.Sender}!" } } } }
                        },
                        new GameModuleConfig {
                            ModuleType = "TimerListener",
                            Parameters = new Dictionary<string, string> { { "DurationSeconds", "60" } },
                            OnTriggerActions = new List<GameActionConfig> { new GameActionConfig { ActionType = "AdvanceStage" } }
                        }
                    },
                    Transitions = new List<GameTransition> { new GameTransition { TargetStageId = "naked_eval", TriggerType = "Manual" } }
                },
                new GameStage {
                    Id = "naked_eval", Name = "Naked: Evaluation", GmDescription = "Broadcasts players who failed the naked threshold.",
                    OnEnterActions = new List<GameActionConfig> {
                        new GameActionConfig {
                            ActionType = "EvaluateThreshold",
                            Parameters = new Dictionary<string, string> {
                                { "Prefix", "naked_score_" },
                                { "Operator", "<=" },
                                { "Threshold", "250" },
                                { "PassMessage", "[Naked Game] {Players}, you scored 250 or less! Strip for 15 minutes!" },
                                { "FailMessage", "[Naked Game] Everyone scored above 250. You are all safe!" }
                            }
                        }
                    },
                    Transitions = new List<GameTransition> { new GameTransition { TargetStageId = "idle", TriggerType = "Auto" } }
                },
                new GameStage {
                    Id = "emote_start", Name = "Emote: Initialization", GmDescription = "Starts the Sexy Emote game setup.",
                    OnEnterActions = new List<GameActionConfig> {
                        new GameActionConfig { ActionType = "ClearVariables", Parameters = new Dictionary<string, string> { { "Prefix", "emote_score_" } } },
                        new GameActionConfig { ActionType = "SetVariable", Parameters = new Dictionary<string, string> { { "TargetVar", "next_game" }, { "Value", "" } } },
                        new GameActionConfig { ActionType = "BroadcastMessage", Parameters = new Dictionary<string, string> { { "Message", "[Sexy Emote Game] Roll your /random! You commit to a sexy emote for 15 minutes if your score is strictly above 800! You have 60 seconds." } } }
                    },
                    Transitions = new List<GameTransition> { new GameTransition { TargetStageId = "emote_listen", TriggerType = "Auto" } }
                },
                new GameStage {
                    Id = "emote_listen", Name = "Emote: Listening", GmDescription = "Listening for /random rolls for 60 seconds.",
                    ActiveModules = new List<GameModuleConfig> {
                        new GameModuleConfig {
                            ModuleType = "DiceListener", ConditionExpressions = new List<string> { "Var.emote_score_{Event.Sender} == null" },
                            OnTriggerActions = new List<GameActionConfig> { new GameActionConfig { ActionType = "SetVariable", Parameters = new Dictionary<string, string> { { "TargetVar", "emote_score_{Event.Sender}" }, { "Value", "{Event.Roll}" } } } }
                        },
                        new GameModuleConfig {
                            ModuleType = "DiceListener", ConditionExpressions = new List<string> { "Var.emote_score_{Event.Sender} != null" },
                            OnTriggerActions = new List<GameActionConfig> { new GameActionConfig { ActionType = "BroadcastMessage", Parameters = new Dictionary<string, string> { { "Message", "[!] You can only roll once, {Event.Sender}!" } } } }
                        },
                        new GameModuleConfig {
                            ModuleType = "TimerListener",
                            Parameters = new Dictionary<string, string> { { "DurationSeconds", "60" } },
                            OnTriggerActions = new List<GameActionConfig> { new GameActionConfig { ActionType = "AdvanceStage" } }
                        }
                    },
                    Transitions = new List<GameTransition> { new GameTransition { TargetStageId = "emote_eval", TriggerType = "Manual" } }
                },
                new GameStage {
                    Id = "emote_eval", Name = "Emote: Evaluation", GmDescription = "Broadcasts players who breached the emote threshold.",
                    OnEnterActions = new List<GameActionConfig> {
                        new GameActionConfig {
                            ActionType = "EvaluateThreshold",
                            Parameters = new Dictionary<string, string> {
                                { "Prefix", "emote_score_" },
                                { "Operator", ">" },
                                { "Threshold", "800" },
                                { "PassMessage", "[Sexy Emote Game] {Players}, you scored over 800! Do a sexy emote for 15 minutes!" },
                                { "FailMessage", "[Sexy Emote Game] Everyone scored 800 or below. You are all safe!" }
                            }
                        }
                    },
                    Transitions = new List<GameTransition> { new GameTransition { TargetStageId = "idle", TriggerType = "Auto" } }
                }
            }
        };
    }
}