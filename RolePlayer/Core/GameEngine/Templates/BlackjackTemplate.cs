namespace RolePlayer.Core.GameEngine.Templates;

using RolePlayer.Core.GameEngine.Contracts;
using RolePlayer.Core.GameEngine.Models;
using System;
using System.Collections.Generic;

public class BlackjackTemplate : IDefaultGameTemplate {
    public string FileName => "Blackjack.json";

    public GameDefinition Build() {
        return new GameDefinition {
            Name = "Dice Blackjack",
            Author = "Almeris",
            Description = "Roll closer to 21 without busting. Use /random 10 to hit or type !stand to pass.",
            AllowChatRegistration = true,
            Parameters = new Dictionary<string, string> { { "TrackScores", "true" } },
            InitialVariables = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase) {
                { "current_player", "" },
                { "played_count", 0 }
            },
            Stages = new List<GameStage> {
                new GameStage {
                    Id = "registration", Name = "Registration", GmDescription = "Wait for players.",
                    OnEnterActions = new List<GameActionConfig> {
                        new GameActionConfig { ActionType = "BroadcastMessage", Parameters = new Dictionary<string, string> { { "Message", "Registration for Dice Blackjack is open! Type !join to participate." } } }
                    },
                    ActiveModules = new List<GameModuleConfig> {
                        new GameModuleConfig { ModuleType = "ChatListener", Parameters = new Dictionary<string, string> { { "Command", "!join" } }, OnTriggerActions = new List<GameActionConfig> { new GameActionConfig { ActionType = "RegisterPlayer" } } }
                    },
                    Transitions = new List<GameTransition> {
                        new GameTransition { TargetStageId = "playing", TriggerType = "Manual", ConditionExpression = "Participants.Count >= 1" }
                    }
                },
                new GameStage {
                    Id = "playing", Name = "Player Turn", GmDescription = "Current player decides to hit or stand.",
                    OnEnterActions = new List<GameActionConfig> {
                        new GameActionConfig { ActionType = "AdvanceTurn", Parameters = new Dictionary<string, string> { { "TargetVar", "current_player" } } },
                        new GameActionConfig { ActionType = "BroadcastMessage", Parameters = new Dictionary<string, string> { { "Message", "{Var.current_player}'s turn! Use /random 10 to hit, or type !stand." } } }
                    },
                    ActiveModules = new List<GameModuleConfig> {
                        new GameModuleConfig {
                            ModuleType = "DiceListener",
                            ConditionExpressions = new List<string> {
                                "Event.Sender == Var.current_player",
                                "Event.MaxRoll == 10"
                            },
                            OnTriggerActions = new List<GameActionConfig> {
                                new GameActionConfig { ActionType = "IncrementVariable", Parameters = new Dictionary<string, string> { { "TargetVar", "score_{Event.Sender}" }, { "Value", "{Event.Roll}" } } },
                                new GameActionConfig { ActionType = "BroadcastMessage", Parameters = new Dictionary<string, string> { { "Message", "{Event.Sender} rolled {Event.Roll}. Total: {Var.score_{Event.Sender}}" } } }
                            }
                        },
                        new GameModuleConfig {
                            ModuleType = "DiceListener",
                            ConditionExpressions = new List<string> {
                                "Event.Sender == Var.current_player",
                                "Event.MaxRoll != 10"
                            },
                            OnTriggerActions = new List<GameActionConfig> {
                                new GameActionConfig { ActionType = "BroadcastMessage", Parameters = new Dictionary<string, string> { { "Message", "Invalid roll, {Event.Sender}! Please use exactly /random 10." } } }
                            }
                        },
                        new GameModuleConfig {
                            ModuleType = "ChatListener", Parameters = new Dictionary<string, string> { { "Command", "!stand" } },
                            ConditionExpressions = new List<string> { "Event.Sender == Var.current_player" },
                            OnTriggerActions = new List<GameActionConfig> {
                                new GameActionConfig { ActionType = "BroadcastMessage", Parameters = new Dictionary<string, string> { { "Message", "{Event.Sender} stands." } } },
                                new GameActionConfig { ActionType = "AdvanceStage" }
                            }
                        }
                    },
                    Transitions = new List<GameTransition> {
                        new GameTransition { TargetStageId = "bust", TriggerType = "OnEvent", ConditionExpression = "Var.score_{Event.Sender} > 21" },
                        new GameTransition { TargetStageId = "perfect", TriggerType = "OnEvent", ConditionExpression = "Var.score_{Event.Sender} == 21" },
                        new GameTransition { TargetStageId = "next_turn", TriggerType = "Manual" }
                    }
                },
                new GameStage {
                    Id = "bust", Name = "Player Busted", GmDescription = "Player went over 21.",
                    OnEnterActions = new List<GameActionConfig> {
                        new GameActionConfig { ActionType = "BroadcastMessage", Parameters = new Dictionary<string, string> { { "Message", "Oh no! {Var.current_player} busted and ends their turn!" } } }
                    },
                    Transitions = new List<GameTransition> {
                        new GameTransition { TargetStageId = "next_turn", TriggerType = "Auto" }
                    }
                },
                new GameStage {
                    Id = "perfect", Name = "Perfect Score", GmDescription = "Player reached exactly 21.",
                    OnEnterActions = new List<GameActionConfig> {
                        new GameActionConfig { ActionType = "BroadcastMessage", Parameters = new Dictionary<string, string> { { "Message", "Blackjack! {Var.current_player} hit exactly 21 and ends their turn!" } } }
                    },
                    Transitions = new List<GameTransition> {
                        new GameTransition { TargetStageId = "next_turn", TriggerType = "Auto" }
                    }
                },
                new GameStage {
                    Id = "resolution", Name = "Results", GmDescription = "Game over.",
                    OnEnterActions = new List<GameActionConfig> {
                        new GameActionConfig { ActionType = "BroadcastScores", Parameters = new Dictionary<string, string> { { "Prefix", "score_" } } },
                        new GameActionConfig { ActionType = "ResolveBlackjackWinner", Parameters = new Dictionary<string, string> { { "ScorePrefix", "score_" }, { "TargetScore", "21" } } }
                    },
                    Transitions = new List<GameTransition>()
                }
            }
        };
    }
}