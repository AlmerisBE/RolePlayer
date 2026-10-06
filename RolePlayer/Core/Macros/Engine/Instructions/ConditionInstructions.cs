namespace RolePlayer.Core.Macros.Engine.Instructions;

using RolePlayer.Core.Emotes.Contracts;
using RolePlayer.Core.Expressions.Contracts;
using RolePlayer.Core.Macros.Engine.Contracts;
using RolePlayer.Core.Macros.Engine.Models;
using System;
using System.Linq;

public class IfInstruction : IMacroInstruction {
    private string conditionExpression;
    private IExpressionEvaluator evaluator;
    private IPlayerStateProvider playerState;
    private IEmoteCache emoteCache;

    public IfInstruction(string conditionExpression, IExpressionEvaluator evaluator, IPlayerStateProvider playerState, IEmoteCache emoteCache) {
        this.conditionExpression = conditionExpression;
        this.evaluator = evaluator;
        this.playerState = playerState;
        this.emoteCache = emoteCache;
    }

    public void Execute(MacroExecutionContext context) {
        bool result = this.evaluator.Evaluate(this.conditionExpression, varName => {
            if (context.Variables.TryGetValue(varName, out var val)) return val;

            if (varName.StartsWith("Emote.Unlocked.", StringComparison.OrdinalIgnoreCase)) {
                string identifier = varName.Substring(15).Trim();
                uint? emoteId = this.ResolveEmoteId(identifier);
                if (emoteId.HasValue) return this.playerState.IsEmoteUnlocked(emoteId.Value);
                return false;
            }

            if (varName.StartsWith("Emote.Active.", StringComparison.OrdinalIgnoreCase)) {
                string identifier = varName.Substring(13).Trim();
                uint? emoteId = this.ResolveEmoteId(identifier);
                if (emoteId.HasValue) return this.playerState.IsEmoteActive(emoteId.Value);
                return false;
            }

            if (varName.Equals("Emote.Active", StringComparison.OrdinalIgnoreCase)) {
                return this.playerState.GetActiveEmoteId();
            }

            return null;
        });

        context.LastConditionResult = result;

        if (!result) {
            var frame = context.CallStack.Peek();
            if (frame.ProgramCounter < frame.Instructions.Count) frame.ProgramCounter++;
        }
    }

    private uint? ResolveEmoteId(string identifier) {
        if (uint.TryParse(identifier, out uint numericId)) return numericId;

        string commandTarget = identifier.StartsWith("/") ? identifier : $"/{identifier}";
        var cached = this.emoteCache.GetCachedEmotes().FirstOrDefault(e => string.Equals(e.CommandAlias, commandTarget, StringComparison.OrdinalIgnoreCase));

        return cached?.Id;
    }
}

public class ElseInstruction : IMacroInstruction {
    public void Execute(MacroExecutionContext context) {
        if (!context.LastConditionResult.HasValue) return;

        bool previousResult = context.LastConditionResult.Value;
        context.LastConditionResult = null;

        if (previousResult) {
            var frame = context.CallStack.Peek();
            if (frame.ProgramCounter < frame.Instructions.Count) {
                frame.ProgramCounter++;
            }
        }
    }
}