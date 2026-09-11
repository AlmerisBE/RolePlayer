namespace RolePlayer.Tests.Core.GameEngine.Engines;

using RolePlayer.Core.GameEngine.Engines;
using RolePlayer.Core.GameEngine.Models;
using System.Collections.Generic;
using Xunit;

public class DeathRollEngineTests {
    private DeathRollEngine CreateEngine() {
        var engine = new DeathRollEngine();
        var config = new GameSessionConfig { Game = new GameDefinition { Parameters = new Dictionary<string, string> { { "StartingRoll", "999" } } } };
        engine.Initialize(config);
        return engine;
    }

    [Fact]
    public void Start_EntersRegistrationPhase_AndBroadcastsJoinInstructions() {
        var engine = this.CreateEngine();
        var broadcasts = new List<string>();
        engine.BroadcastRequested += msg => broadcasts.Add(msg);

        engine.Start();

        Assert.True(engine.IsRunning);
        Assert.Contains(broadcasts, b => b.Contains("!join"));
        Assert.Contains(broadcasts, b => b.Contains("!start"));
    }

    [Fact]
    public void ProcessEvent_WithJoinCommand_AddsPlayerAndBroadcasts() {
        var engine = this.CreateEngine();
        engine.Start();
        var broadcasts = new List<string>();
        engine.BroadcastRequested += msg => broadcasts.Add(msg);

        engine.ProcessEvent(new ChatGameEvent { Sender = "John Doe", Message = "!join", Channel = GameChatChannel.Say });

        Assert.Contains(broadcasts, b => b.Contains("John Doe joined"));
    }

    [Fact]
    public void ProcessEvent_WithStartCommand_AndNotEnoughPlayers_BroadcastsWarning() {
        var engine = this.CreateEngine();
        engine.Start();
        engine.ProcessEvent(new ChatGameEvent { Sender = "John Doe", Message = "!join", Channel = GameChatChannel.Say });

        var broadcasts = new List<string>();
        engine.BroadcastRequested += msg => broadcasts.Add(msg);

        engine.ProcessEvent(new ChatGameEvent { Sender = "John Doe", Message = "!start", Channel = GameChatChannel.Say });

        Assert.Contains(broadcasts, b => b.Contains("at least 2 players"));
    }

    [Fact]
    public void ProcessEvent_WithStartCommand_AndEnoughPlayers_TransitionsToRollingPhase() {
        var engine = this.CreateEngine();
        engine.Start();
        engine.ProcessEvent(new ChatGameEvent { Sender = "John Doe", Message = "!join", Channel = GameChatChannel.Say });
        engine.ProcessEvent(new ChatGameEvent { Sender = "Jane Doe", Message = "!join", Channel = GameChatChannel.Say });

        var broadcasts = new List<string>();
        engine.BroadcastRequested += msg => broadcasts.Add(msg);

        engine.ProcessEvent(new ChatGameEvent { Sender = "John Doe", Message = "!start", Channel = GameChatChannel.Say });

        Assert.Contains(broadcasts, b => b.Contains("game begins"));
        Assert.Contains(broadcasts, b => b.Contains("/random 999"));
    }

    [Fact]
    public void ProcessEvent_WithDiceRoll_FromNonParticipant_IsIgnored() {
        var engine = this.CreateEngine();
        engine.Start();
        engine.ProcessEvent(new ChatGameEvent { Sender = "John Doe", Message = "!join", Channel = GameChatChannel.Say });
        engine.ProcessEvent(new ChatGameEvent { Sender = "Jane Doe", Message = "!join", Channel = GameChatChannel.Say });
        engine.ProcessEvent(new ChatGameEvent { Sender = "John Doe", Message = "!start", Channel = GameChatChannel.Say });

        var broadcasts = new List<string>();
        engine.BroadcastRequested += msg => broadcasts.Add(msg);

        // Un joueur non-inscrit tente de jeter les dés
        engine.ProcessEvent(new DiceRollGameEvent { Sender = "Intruder", Roll = 42, OutOf = 999 });

        Assert.Empty(broadcasts);
    }

    [Fact]
    public void ProcessEvent_WithDiceRollEqualTo1_BroadcastsLossAndFinishesGame() {
        var engine = this.CreateEngine();
        engine.Start();
        engine.ProcessEvent(new ChatGameEvent { Sender = "John Doe", Message = "!join", Channel = GameChatChannel.Say });
        engine.ProcessEvent(new ChatGameEvent { Sender = "Jane Doe", Message = "!join", Channel = GameChatChannel.Say });
        engine.ProcessEvent(new ChatGameEvent { Sender = "John Doe", Message = "!start", Channel = GameChatChannel.Say });

        var broadcasts = new List<string>();
        engine.BroadcastRequested += msg => broadcasts.Add(msg);

        bool finished = false;
        engine.GameFinished += () => finished = true;

        engine.ProcessEvent(new DiceRollGameEvent { Sender = "Jane Doe", Roll = 1, OutOf = 999 });

        Assert.Contains(broadcasts, b => b.Contains("Jane Doe"));
        Assert.Contains(broadcasts, b => b.Contains("died"));
        Assert.True(finished);
        Assert.False(engine.IsRunning);
    }
}