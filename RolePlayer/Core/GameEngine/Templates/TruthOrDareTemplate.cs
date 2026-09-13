namespace RolePlayer.Core.GameEngine.Templates;

using RolePlayer.Core.GameEngine.Contracts;
using RolePlayer.Core.GameEngine.Models;
using System;
using System.Collections.Generic;

public class TruthOrDareTemplate : IDefaultGameTemplate {
    public string FileName => "TruthOrDare.json";

    public GameDefinition Build() {
        return new GameDefinition {
            Name = "Truth or Dare",
            Author = "Almeris",
            Description = "Everyone rolls. Highest score asks Truth or Dare to the lowest score.",
            AllowChatRegistration = true,
            InitialVariables = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase) { { "highest_roll", 0 }, { "lowest_roll", 999 }, { "highest_player", "" }, { "lowest_player", "" }, { "rolls_count", 0 } },
            Stages = new List<GameStage> {
                new GameStage {
                    Id = "registration", Name = "Registration", GmDescription = "Wait for players to !join.",
                    OnEnterActions = new List<GameActionConfig> { new GameActionConfig { ActionType = "BroadcastMessage", Parameters = new Dictionary<string, string> { { "Message", "Registration for Truth or Dare is open! Type !join to participate." } } } },
                    ActiveModules = new List<GameModuleConfig> { new GameModuleConfig { ModuleType = "ChatListener", Parameters = new Dictionary<string, string> { { "Command", "!join" } }, OnTriggerActions = new List<GameActionConfig> { new GameActionConfig { ActionType = "RegisterPlayer" } } } },
                    Transitions = new List<GameTransition> { new GameTransition { TargetStageId = "rolling", TriggerType = "Manual", ConditionExpression = "Participants.Count >= 2" } }
                },
                new GameStage {
                    Id = "rolling", Name = "Rolling Phase", GmDescription = "Everyone rolls. The engine tracks the highest and lowest.",
                    OnEnterActions = new List<GameActionConfig> {
                        new GameActionConfig { ActionType = "SetVariable", Parameters = new Dictionary<string, string> { { "TargetVar", "highest_roll" }, { "Value", "0" } } },
                        new GameActionConfig { ActionType = "SetVariable", Parameters = new Dictionary<string, string> { { "TargetVar", "lowest_roll" }, { "Value", "999" } } },
                        new GameActionConfig { ActionType = "SetVariable", Parameters = new Dictionary<string, string> { { "TargetVar", "rolls_count" }, { "Value", "0" } } },
                        new GameActionConfig { ActionType = "BroadcastMessage", Parameters = new Dictionary<string, string> { { "Message", "Time to roll! Everyone use /random 100!" } } }
                    },
                    ActiveModules = new List<GameModuleConfig> {
                        new GameModuleConfig { ModuleType = "DiceListener", ConditionExpressions = new List<string> { "Participants CONTAINS Event.Sender", "Event.Roll > Var.highest_roll" }, OnTriggerActions = new List<GameActionConfig> { new GameActionConfig { ActionType = "SetVariable", Parameters = new Dictionary<string, string> { { "TargetVar", "highest_roll" }, { "Value", "{Event.Roll}" } } }, new GameActionConfig { ActionType = "SetVariable", Parameters = new Dictionary<string, string> { { "TargetVar", "highest_player" }, { "Value", "{Event.Sender}" } } } } },
                        new GameModuleConfig { ModuleType = "DiceListener", ConditionExpressions = new List<string> { "Participants CONTAINS Event.Sender", "Event.Roll < Var.lowest_roll" }, OnTriggerActions = new List<GameActionConfig> { new GameActionConfig { ActionType = "SetVariable", Parameters = new Dictionary<string, string> { { "TargetVar", "lowest_roll" }, { "Value", "{Event.Roll}" } } }, new GameActionConfig { ActionType = "SetVariable", Parameters = new Dictionary<string, string> { { "TargetVar", "lowest_player" }, { "Value", "{Event.Sender}" } } } } },
                        new GameModuleConfig { ModuleType = "DiceListener", ConditionExpressions = new List<string> { "Participants CONTAINS Event.Sender" }, OnTriggerActions = new List<GameActionConfig> { new GameActionConfig { ActionType = "IncrementVariable", Parameters = new Dictionary<string, string> { { "TargetVar", "rolls_count" }, { "Value", "1" } } } } }
                    },
                    Transitions = new List<GameTransition> { new GameTransition { TargetStageId = "resolution", TriggerType = "OnEvent", ConditionExpression = "Var.rolls_count == Participants.Count" } }
                },
                new GameStage {
                    Id = "resolution", Name = "Resolution", GmDescription = "Results are announced.",
                    OnEnterActions = new List<GameActionConfig> { new GameActionConfig { ActionType = "BroadcastMessage", Parameters = new Dictionary<string, string> { { "Message", "Results: {Var.highest_player} ({Var.highest_roll}) VS {Var.lowest_player} ({Var.lowest_roll})! {Var.highest_player}, what is your Truth or Dare?" } } } },
                    Transitions = new List<GameTransition> { new GameTransition { TargetStageId = "rolling", TriggerType = "Manual" } }
                }
            }
        };
    }
}