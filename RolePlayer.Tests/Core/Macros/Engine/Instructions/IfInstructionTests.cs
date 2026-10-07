namespace RolePlayer.Tests.Core.Macros.Engine.Instructions;

using NSubstitute;
using RolePlayer.Core.Emotes.Contracts;
using RolePlayer.Core.Emotes.Models;
using RolePlayer.Core.Expressions.Services;
using RolePlayer.Core.Macros.Engine.Contracts;
using RolePlayer.Core.Macros.Engine.Instructions;
using RolePlayer.Core.Macros.Engine.Models;
using System.Collections.Generic;
using Xunit;

public class IfInstructionTests {
    [Fact]
    public void Execute_ResolvesEmoteByChatCommand_AndEvaluatesImplicitBoolean() {
        var evaluator = new ExpressionEvaluator();
        var playerState = Substitute.For<IPlayerStateProvider>();
        var emoteCache = Substitute.For<IEmoteCache>();

        var emotes = new List<EnrichedEmote> {
            new EnrichedEmote { Id = 50, EnglishCommand = "/dance", UnlockLink = 42 }
        };
        emoteCache.GetCachedEmotes().Returns(emotes);

        playerState.IsEmoteUnlocked(50).Returns(true);

        var instruction = new IfInstruction("{Emote.Unlocked./dance}", evaluator, playerState, emoteCache);
        var context = new MacroExecutionContext();

        var frame = new MacroCallFrame {
            Instructions = new List<IMacroInstruction> { instruction, Substitute.For<IMacroInstruction>() }
        };
        context.CallStack.Push(frame);

        instruction.Execute(context);

        Assert.Equal(0, frame.ProgramCounter);
        Assert.True(context.LastConditionResult);
    }

    [Fact]
    public void Execute_ImplicitBooleanWithNotOperator_SkipsNextLineWhenTrue() {
        var evaluator = new ExpressionEvaluator();
        var playerState = Substitute.For<IPlayerStateProvider>();
        var emoteCache = Substitute.For<IEmoteCache>();

        var emotes = new List<EnrichedEmote> {
            new EnrichedEmote { Id = 50, EnglishCommand = "/dance", UnlockLink = 42 }
        };
        emoteCache.GetCachedEmotes().Returns(emotes);

        playerState.IsEmoteUnlocked(50).Returns(true);

        var instruction = new IfInstruction("!{Emote.Unlocked.dance}", evaluator, playerState, emoteCache);
        var context = new MacroExecutionContext();

        var frame = new MacroCallFrame {
            Instructions = new List<IMacroInstruction> { instruction, Substitute.For<IMacroInstruction>() }
        };
        context.CallStack.Push(frame);

        instruction.Execute(context);

        Assert.Equal(1, frame.ProgramCounter);
        Assert.False(context.LastConditionResult);
    }
}