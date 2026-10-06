namespace RolePlayer.Core.Macros.Engine.Parsers;

using RolePlayer.Core.Emotes.Contracts;
using RolePlayer.Core.Expressions.Contracts;
using RolePlayer.Core.Macros.Engine.Contracts;
using RolePlayer.Core.Macros.Engine.Instructions;
using System;

public class IfParser : IInstructionParser {
    private IExpressionEvaluator evaluator;
    private IPlayerStateProvider playerState;
    private IEmoteCache emoteCache;

    public IfParser(IExpressionEvaluator evaluator, IPlayerStateProvider playerState, IEmoteCache emoteCache) {
        this.evaluator = evaluator;
        this.playerState = playerState;
        this.emoteCache = emoteCache;
    }

    public bool TryParse(string line, out IMacroInstruction? instruction, out string errorMessage) {
        instruction = null;
        errorMessage = string.Empty;
        if (!line.StartsWith("# if ", StringComparison.OrdinalIgnoreCase)) return false;

        var condition = line.Substring(5).Trim();
        if (this.evaluator.Validate(condition, out errorMessage)) {
            instruction = new IfInstruction(condition, this.evaluator, this.playerState, this.emoteCache);
        }

        return true;
    }
}

public class ElseParser : IInstructionParser {
    public bool TryParse(string line, out IMacroInstruction? instruction, out string errorMessage) {
        instruction = null;
        errorMessage = string.Empty;
        if (!line.Equals("# else", StringComparison.OrdinalIgnoreCase)) return false;

        instruction = new ElseInstruction();
        return true;
    }
}