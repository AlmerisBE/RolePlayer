namespace RolePlayer.Core.GameEngine.Templates;

using RolePlayer.Core.GameEngine.Contracts;
using RolePlayer.Core.GameEngine.Models;
using System;
using System.Collections.Generic;

public class RiddlesTemplate : IDefaultGameTemplate {
    public string FileName => "Riddles.json";

    public GameDefinition Build() {
        return new GameDefinition {
            Name = "Emote Riddles",
            Author = "Almeris",
            Description = "Answer the riddle by performing the correct emote before time runs out! First to reach the max score wins.",
            AllowChatRegistration = true,
            Parameters = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase) { { "TrackScores", "true" } },
            InitialVariables = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase) { { "expected_emote_id", 0 }, { "riddle_text", "I am joy incarnate!" }, { "round_duration", 30 }, { "max_score", 3 } },
            ExposedVariables = new List<GameVariableDefinition> {
                new GameVariableDefinition { Key = "expected_emote_id", Label = "Expected Emote (Answer)", Type = "Emote" },
                new GameVariableDefinition { Key = "riddle_text", Label = "Riddle Text", Type = "String" },
                new GameVariableDefinition { Key = "round_duration", Label = "Round Duration (Seconds)", Type = "Number" },
                new GameVariableDefinition { Key = "max_score", Label = "Score to Win", Type = "Number" }
            },
            Stages = new List<GameStage> {
                new GameStage {
                    Id = "registration", Name = "Registration & Preparation", GmDescription = "Wait for players to !join. Configure your riddle in the UI above, then manually transition.",
                    OnEnterActions = new List<GameActionConfig> { new GameActionConfig { ActionType = "BroadcastMessage", Parameters = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase) { { "Message", "Registration for Emote Riddles is open! Type !join to participate." } } } },
                    ActiveModules = new List<GameModuleConfig> { new GameModuleConfig { ModuleType = "ChatListener", Parameters = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase) { { "Command", "!join" } }, OnTriggerActions = new List<GameActionConfig> { new GameActionConfig { ActionType = "RegisterPlayer" } } } },
                    Transitions = new List<GameTransition> { new GameTransition { TargetStageId = "countdown", TriggerType = "Manual", ConditionExpression = "Participants.Count >= 1" } }
                },
                new GameStage {
                    Id = "countdown", Name = "Countdown", GmDescription = "Broadcasts the riddle with a countdown. Automatically advances to Action Phase after 4 seconds.",
                    OnEnterActions = new List<GameActionConfig> { new GameActionConfig { ActionType = "BroadcastMessage", Parameters = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase) { { "Message", "The riddle begins in 3...\\n2...\\n1...\\n[Riddle] {Var.riddle_text}" } } } },
                    ActiveModules = new List<GameModuleConfig> { new GameModuleConfig { ModuleType = "TimerListener", Parameters = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase) { { "DurationSeconds", "5" } }, OnTriggerActions = new List<GameActionConfig> { new GameActionConfig { ActionType = "AdvanceStage" } } } },
                    Transitions = new List<GameTransition> { new GameTransition { TargetStageId = "playing", TriggerType = "Manual" } }
                },
                new GameStage {
                    Id = "playing", Name = "Action Phase", GmDescription = "Engine listens for the correct emote and tracks time.",
                    OnEnterActions = new List<GameActionConfig> { new GameActionConfig { ActionType = "BroadcastMessage", Parameters = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase) { { "Message", "[Start] You have {Var.round_duration} seconds to perform the emote!" } } } },
                    ActiveModules = new List<GameModuleConfig> {
                        new GameModuleConfig { ModuleType = "ChatListener", Parameters = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase) { { "Command", "!score" } }, OnTriggerActions = new List<GameActionConfig> { new GameActionConfig { ActionType = "BroadcastScores", Parameters = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase) { { "Prefix", "score_" } } } } },
                        new GameModuleConfig {
                            ModuleType = "EmoteListener",
                            ConditionExpressions = new List<string> { "Participants CONTAINS Event.Sender", "Var.expected_emote_id != 0", "Event.EmoteId == Var.expected_emote_id" },
                            OnTriggerActions = new List<GameActionConfig> {
                                new GameActionConfig { ActionType = "BroadcastMessage", Parameters = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase) { { "Message", "[Winner] {Event.Sender} found the correct emote! Well done!" } } },
                                new GameActionConfig { ActionType = "IncrementVariable", Parameters = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase) { { "TargetVar", "score_{Event.Sender}" }, { "Value", "1" } } },
                                new GameActionConfig { ActionType = "EndGameIfScoreReached", Parameters = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase) { { "Prefix", "score_" }, { "TargetScore", "{Var.max_score}" } } },
                                new GameActionConfig { ActionType = "SetVariable", Parameters = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase) { { "TargetVar", "expected_emote_id" }, { "Value", "0" } } },
                                new GameActionConfig { ActionType = "AdvanceStage" }
                            }
                        },
                        new GameModuleConfig { ModuleType = "TimerListener", Parameters = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase) { { "DurationSeconds", "{Var.round_duration}" } }, OnTriggerActions = new List<GameActionConfig> { new GameActionConfig { ActionType = "BroadcastMessage", Parameters = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase) { { "Message", "[Time's Up] No one found the answer in time!" } } }, new GameActionConfig { ActionType = "AdvanceStage" } } }
                    },
                    Transitions = new List<GameTransition> { new GameTransition { TargetStageId = "round_end", TriggerType = "Manual" } }
                },
                new GameStage {
                    Id = "round_end", Name = "Round Ended", GmDescription = "Prepare the next riddle and return to Countdown, or Stop the Session.",
                    ActiveModules = new List<GameModuleConfig> { new GameModuleConfig { ModuleType = "ChatListener", Parameters = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase) { { "Command", "!score" } }, OnTriggerActions = new List<GameActionConfig> { new GameActionConfig { ActionType = "BroadcastScores", Parameters = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase) { { "Prefix", "score_" } } } } } },
                    Transitions = new List<GameTransition> { new GameTransition { TargetStageId = "countdown", TriggerType = "Manual" } }
                }
            }
        };
    }
}