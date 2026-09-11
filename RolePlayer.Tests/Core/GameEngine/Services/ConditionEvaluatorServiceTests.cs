namespace RolePlayer.Tests.Core.GameEngine.Services;

using RolePlayer.Core.GameEngine.Models;
using RolePlayer.Core.GameEngine.Services;
using Xunit;

public class ConditionEvaluatorServiceTests {
    private ConditionEvaluatorService evaluator = new();

    [Fact]
    public void Evaluate_ParticipantCount_ReturnsTrueWhenConditionMet() {
        var context = new GameSessionContext();
        context.Participants.Add("Player1");
        context.Participants.Add("Player2");

        bool result = this.evaluator.Evaluate("Participants.Count >= 2", context);

        Assert.True(result);
    }

    [Fact]
    public void Evaluate_EventSenderNotContains_ReturnsTrueWhenUnregistered() {
        var context = new GameSessionContext();
        context.Participants.Add("Player1");
        context.CurrentEvent = new ChatGameEvent { Sender = "Player2", Message = "!join" };

        bool result = this.evaluator.Evaluate("Participants NOT_CONTAINS Event.Sender", context);

        Assert.True(result);
    }

    [Fact]
    public void Evaluate_VariableComparison_EvaluatesCorrectly() {
        var context = new GameSessionContext();
        context.Variables["current_max_roll"] = 999;
        context.CurrentEvent = new DiceRollGameEvent { Sender = "Player1", Roll = 50, OutOf = 999 };

        bool result = this.evaluator.Evaluate("Event.OutOf == Var.current_max_roll", context);

        Assert.True(result);
    }

    [Fact]
    public void Evaluate_RollEqualsOne_ReturnsTrue() {
        var context = new GameSessionContext();
        context.CurrentEvent = new DiceRollGameEvent { Sender = "Player1", Roll = 1, OutOf = 50 };

        bool result = this.evaluator.Evaluate("Event.Roll == 1", context);

        Assert.True(result);
    }

    [Fact]
    public void EvaluateAll_WithMultipleConditions_ReturnsTrueOnlyIfAllPass() {
        var context = new GameSessionContext();
        context.Participants.Add("Player1");
        context.Variables["is_active"] = 1;
        context.CurrentEvent = new ChatGameEvent { Sender = "Player1" };

        var conditions = new[] {
            "Participants CONTAINS Event.Sender",
            "Var.is_active == 1"
        };

        bool result = this.evaluator.EvaluateAll(conditions, context);

        Assert.True(result);
    }
}