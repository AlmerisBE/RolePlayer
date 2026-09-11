namespace RolePlayer.Tests.Core.GameEngine.Services;

using RolePlayer.Core.GameEngine.Models;
using RolePlayer.Core.GameEngine.Services;
using System.Collections.Generic;
using Xunit;

public class GameActionExecutionServiceTests {
    private GameActionExecutionService actionService = new();

    [Fact]
    public void Execute_RegisterPlayer_AddsSenderToParticipants() {
        var context = new GameSessionContext();
        context.CurrentEvent = new ChatGameEvent { Sender = "John Doe" };

        var action = new GameActionConfig { ActionType = "RegisterPlayer" };

        this.actionService.Execute(action, context);

        Assert.Single(context.Participants);
        Assert.Contains("John Doe", context.Participants);
    }

    [Fact]
    public void Execute_SetVariable_UpdatesContextVariable() {
        var context = new GameSessionContext();
        context.CurrentEvent = new DiceRollGameEvent { Sender = "Jane", Roll = 42, OutOf = 999 };

        var action = new GameActionConfig {
            ActionType = "SetVariable",
            Parameters = new Dictionary<string, string> {
                { "TargetVar", "current_max_roll" },
                { "Value", "{Event.Roll}" }
            }
        };

        this.actionService.Execute(action, context);

        Assert.True(context.Variables.ContainsKey("current_max_roll"));
        Assert.Equal("42", context.Variables["current_max_roll"].ToString());
    }

    [Fact]
    public void Execute_BroadcastMessage_ReplacesPlaceholdersAndFiresEvent() {
        var context = new GameSessionContext();
        context.Participants.Add("Player1");
        context.Participants.Add("Player2");
        context.CurrentEvent = new ChatGameEvent { Sender = "Player3" };

        var action = new GameActionConfig {
            ActionType = "BroadcastMessage",
            Parameters = new Dictionary<string, string> {
                { "Message", "{Event.Sender} joined! We now have {Participants.Count} players." }
            }
        };

        string? broadcastedMessage = null;
        this.actionService.BroadcastRequested += msg => broadcastedMessage = msg;

        this.actionService.Execute(action, context);

        Assert.Equal("Player3 joined! We now have 2 players.", broadcastedMessage);
    }

    [Fact]
    public void Execute_AdvanceStage_FiresStageAdvanceEvent() {
        var context = new GameSessionContext();
        var action = new GameActionConfig { ActionType = "AdvanceStage" };

        bool advanceRequested = false;
        this.actionService.StageAdvanceRequested += () => advanceRequested = true;

        this.actionService.Execute(action, context);

        Assert.True(advanceRequested);
    }
}