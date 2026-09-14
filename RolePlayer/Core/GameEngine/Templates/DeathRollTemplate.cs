namespace RolePlayer.Core.GameEngine.Templates;

using RolePlayer.Core.GameEngine.Contracts;
using RolePlayer.Core.GameEngine.Models;
using System;
using System.Collections.Generic;

public class DeathRollTemplate : IDefaultGameTemplate {
    public string FileName => "DeathRoll.json";

    public GameDefinition Build() {
        return new GameDefinition {
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
                        new GameActionConfig { ActionType = "BroadcastMessage", Parameters = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase) { { "Message", "Registration is open! Type !join to participate." } } }
                    },
                    ActiveModules = new List<GameModuleConfig> {
                        new GameModuleConfig { ModuleType = "ChatListener", Parameters = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase) { { "Command", "!join" } }, OnTriggerActions = new List<GameActionConfig> { new GameActionConfig { ActionType = "RegisterPlayer" } } }
                    },
                    Transitions = new List<GameTransition> { new GameTransition { TargetStageId = "playing", TriggerType = "Manual", ConditionExpression = "Participants.Count >= 2" } }
                },
                new GameStage {
                    Id = "playing",
                    Name = "In Progress",
                    GmDescription = "Game is running. Engine tracks max roll and turns. First to roll 1 loses.",
                    OnEnterActions = new List<GameActionConfig> {
                        new GameActionConfig { ActionType = "AdvanceTurn", Parameters = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase) { { "TargetVar", "current_player" } } },
                        new GameActionConfig { ActionType = "BroadcastMessage", Parameters = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase) { { "Message", "[Start] Game begins! First to roll 1 loses. {Var.current_player}, you're up! (/random {Var.current_max_roll})" } } }
                    },
                    ActiveModules = new List<GameModuleConfig> {
                        new GameModuleConfig { ModuleType = "DiceListener", ConditionExpressions = new List<string> { "Participants CONTAINS Event.Sender", "Event.Sender != Var.current_player" }, OnTriggerActions = new List<GameActionConfig> { new GameActionConfig { ActionType = "BroadcastMessage", Parameters = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase) { { "Message", "[!] Not your turn, {Event.Sender}! Waiting for {Var.current_player}." } } } } },
                        new GameModuleConfig { ModuleType = "DiceListener", ConditionExpressions = new List<string> { "Participants CONTAINS Event.Sender", "Event.Sender == Var.current_player", "Event.MaxRoll != Var.current_max_roll" }, OnTriggerActions = new List<GameActionConfig> { new GameActionConfig { ActionType = "BroadcastMessage", Parameters = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase) { { "Message", "[!] Invalid roll, {Event.Sender}! Must roll out of {Var.current_max_roll}! (/random {Var.current_max_roll})" } } } } },
                        new GameModuleConfig { ModuleType = "DiceListener", ConditionExpressions = new List<string> { "Participants CONTAINS Event.Sender", "Event.Sender == Var.current_player", "Event.MaxRoll == Var.current_max_roll", "Event.Roll != 1" }, OnTriggerActions = new List<GameActionConfig> { new GameActionConfig { ActionType = "SetVariable", Parameters = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase) { { "TargetVar", "current_max_roll" }, { "Value", "{Event.Roll}" } } }, new GameActionConfig { ActionType = "AdvanceTurn", Parameters = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase) { { "TargetVar", "current_player" } } }, new GameActionConfig { ActionType = "BroadcastMessage", Parameters = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase) { { "Message", "[Roll] {Event.Sender} rolled {Event.Roll}. Next up: {Var.current_player} (/random {Var.current_max_roll})" } } } } },
                        new GameModuleConfig { ModuleType = "DiceListener", ConditionExpressions = new List<string> { "Participants CONTAINS Event.Sender", "Event.Sender == Var.current_player", "Event.MaxRoll == Var.current_max_roll", "Event.Roll == 1" }, OnTriggerActions = new List<GameActionConfig> { new GameActionConfig { ActionType = "BroadcastMessage", Parameters = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase) { { "Message", "[Game Over] {Event.Sender} rolled a 1 and died!" } } }, new GameActionConfig { ActionType = "StopGame" } } }
                    }
                }
            }
        };
    }
}