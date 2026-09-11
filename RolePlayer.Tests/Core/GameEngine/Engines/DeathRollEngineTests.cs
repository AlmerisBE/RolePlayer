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

        var broadcasts = new List<string>();
        engine.BroadcastRequested += msg => broadcasts.Add(msg);

        engine.Start();

        Assert.True(engine.IsRunning);
        Assert.Contains(broadcasts, b => b.Contains("Welcome to Death Roll"));
        Assert.Contains(broadcasts, b => b.Contains("/random 500"));
    }

    [Fact]
    public void ProcessEvent_WithValidDiceRoll_UpdatesCurrentMaxAndBroadcastsNextTurn() {
        var engine = new DeathRollEngine();
        var config = new GameSessionConfig { Game = new GameDefinition { Parameters = new Dictionary<string, string> { { "StartingRoll", "999" } } } };
        engine.Initialize(config);
        engine.Start();

        var broadcasts = new List<string>();
        engine.BroadcastRequested += msg => broadcasts.Add(msg);

        var diceEvent = new DiceRollGameEvent { Sender = "John Doe", Roll = 42, OutOf = 999 };
        engine.ProcessEvent(diceEvent);

        Assert.Contains(broadcasts, b => b.Contains("John Doe"));
        Assert.Contains(broadcasts, b => b.Contains("/random 42"));
        Assert.True(engine.IsRunning);
    }

    [Fact]
    public void ProcessEvent_WithDiceRollEqualTo1_BroadcastsLossAndFinishesGame() {
        var engine = new DeathRollEngine();
        var config = new GameSessionConfig { Game = new GameDefinition { Parameters = new Dictionary<string, string> { { "StartingRoll", "999" } } } };
        engine.Initialize(config);
        engine.Start();

        var broadcasts = new List<string>();
        engine.BroadcastRequested += msg => broadcasts.Add(msg);

        bool finished = false;
        engine.GameFinished += () => finished = true;

        var diceEvent = new DiceRollGameEvent { Sender = "Jane Doe", Roll = 1, OutOf = 999 };
        engine.ProcessEvent(diceEvent);

        Assert.Contains(broadcasts, b => b.Contains("Jane Doe"));
        Assert.Contains(broadcasts, b => b.Contains("died"));
        Assert.True(finished);
        Assert.False(engine.IsRunning);
    }
}