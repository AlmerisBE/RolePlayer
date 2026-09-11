namespace RolePlayer.Tests.Core.GameEngine.Engines;

using RolePlayer.Core.GameEngine.Engines;
using RolePlayer.Core.GameEngine.Models;
using System.Collections.Generic;
using Xunit;

public class DeathRollEngineTests {
    private DeathRollEngine CreateEngine(bool allowJoin = true, Dictionary<string, string>? messages = null) {
        var engine = new DeathRollEngine();
        var config = new GameSessionConfig {
            Game = new GameDefinition {
                AllowChatRegistration = allowJoin,
                Messages = messages ?? new Dictionary<string, string>(),
                Parameters = new Dictionary<string, string> { { "StartingRoll", "999" } }
            }
        };
        engine.Initialize(config);
        return engine;
    }

    [Fact]
    public void ProcessEvent_WithJoinCommand_WhenNotAllowed_IgnoresJoin() {
        var engine = this.CreateEngine(allowJoin: false);
        engine.Start();

        engine.ProcessEvent(new ChatGameEvent { Sender = "John Doe", Message = "!join", Channel = GameChatChannel.Say });

        Assert.Empty(engine.Participants);
    }

    [Fact]
    public void AddParticipant_Manually_AddsPlayerSuccessfully() {
        var engine = this.CreateEngine(allowJoin: false);
        engine.Start();

        engine.AddParticipant("Jane Doe");

        Assert.Single(engine.Participants);
        Assert.Equal("Jane Doe", engine.Participants[0]);
    }

    [Fact]
    public void AdvanceStage_TransitionsThroughStandardPhases() {
        var engine = this.CreateEngine();
        engine.Start();

        Assert.Equal("Preparation", engine.CurrentStageName);
        engine.AdvanceStage();

        Assert.Equal("Registration", engine.CurrentStageName);
        engine.AdvanceStage();

        Assert.Equal("InProgress", engine.CurrentStageName);
    }

    [Fact]
    public void AllowChatRegistration_Setter_BroadcastsStatusMessage() {
        var engine = this.CreateEngine(allowJoin: false);
        engine.Start();

        var broadcasts = new List<string>();
        engine.BroadcastRequested += msg => broadcasts.Add(msg);

        engine.AllowChatRegistration = true;
        Assert.Contains(broadcasts, b => b.Contains("open"));

        broadcasts.Clear();

        engine.AllowChatRegistration = false;
        Assert.Contains(broadcasts, b => b.Contains("closed"));
    }

    [Fact]
    public void Start_UsesCustomMessages_IfDefinedInJson() {
        var customMsgs = new Dictionary<string, string> { { "Msg_Welcome", "Bienvenue au Death Roll !" } };
        var engine = this.CreateEngine(messages: customMsgs);

        var broadcasts = new List<string>();
        engine.BroadcastRequested += msg => broadcasts.Add(msg);

        engine.Start();

        Assert.Contains(broadcasts, b => b.Contains("Bienvenue au Death Roll !"));
    }
}