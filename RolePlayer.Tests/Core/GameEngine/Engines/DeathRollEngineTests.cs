namespace RolePlayer.Tests.Core.GameEngine.Engines;

using RolePlayer.Core.GameEngine.Engines;
using RolePlayer.Core.GameEngine.Models;
using System.Collections.Generic;
using Xunit;

public class DeathRollEngineTests {
    [Fact]
    public void Start_BroadcastsInitialMessage_WithConfiguredStartingRoll() {
        var engine = new DeathRollEngine();
        var config = new GameSessionConfig { Game = new GameDefinition { Parameters = new Dictionary<string, string> { { "StartingRoll", "500" } } } };
        engine.Initialize(config);

        string? broadcast = null;
        engine.BroadcastRequested += msg => broadcast = msg;

        engine.Start();

        Assert.True(engine.IsRunning);
        Assert.Contains("500", broadcast);
    }

    [Fact]
    public void ProcessEvent_WithValidDiceRoll_UpdatesCurrentMaxAndBroadcastsNextTurn() {
        var engine = new DeathRollEngine();
        var config = new GameSessionConfig { Game = new GameDefinition { Parameters = new Dictionary<string, string> { { "StartingRoll", "999" } } } };
        engine.Initialize(config);
        engine.Start();

        string? broadcast = null;
        engine.BroadcastRequested += msg => broadcast = msg;

        var diceEvent = new DiceRollGameEvent { Sender = "John Doe", Roll = 42, OutOf = 999 };
        engine.ProcessEvent(diceEvent);

        Assert.Contains("42", broadcast);
        Assert.True(engine.IsRunning);
    }

    [Fact]
    public void ProcessEvent_WithDiceRollEqualTo1_BroadcastsLossAndFinishesGame() {
        var engine = new DeathRollEngine();
        var config = new GameSessionConfig { Game = new GameDefinition { Parameters = new Dictionary<string, string> { { "StartingRoll", "999" } } } };
        engine.Initialize(config);
        engine.Start();

        string? broadcast = null;
        engine.BroadcastRequested += msg => broadcast = msg;

        bool finished = false;
        engine.GameFinished += () => finished = true;

        var diceEvent = new DiceRollGameEvent { Sender = "John Doe", Roll = 1, OutOf = 999 };
        engine.ProcessEvent(diceEvent);

        Assert.Contains("rolled a 1", broadcast);
        Assert.True(finished);
        Assert.False(engine.IsRunning);
    }

    [Fact]
    public void ProcessEvent_WithInvalidOutOf_IgnoresRoll() {
        var engine = new DeathRollEngine();
        var config = new GameSessionConfig { Game = new GameDefinition { Parameters = new Dictionary<string, string> { { "StartingRoll", "999" } } } };
        engine.Initialize(config);
        engine.Start();

        string? broadcast = null;
        engine.BroadcastRequested += msg => broadcast = msg;

        // The current max is 999, but the player rolled out of 100. It should be ignored.
        var diceEvent = new DiceRollGameEvent { Sender = "John Doe", Roll = 50, OutOf = 100 };
        engine.ProcessEvent(diceEvent);

        Assert.Null(broadcast); // No new broadcast triggered
    }
}