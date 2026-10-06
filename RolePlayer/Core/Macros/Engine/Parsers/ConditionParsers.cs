namespace RolePlayer.Core.Macros.Engine.Parsers;

using RolePlayer.Core.Expressions.Contracts;
using RolePlayer.Core.Macros.Engine.Contracts;
using RolePlayer.Core.Macros.Engine.Instructions;
using System;

public class IfParser : IInstructionParser {
    private IExpressionEvaluator evaluator;

    public IfParser(IExpressionEvaluator evaluator) {
        this.evaluator = evaluator;
    }

    public bool TryParse(string line, out IMacroInstruction? instruction) {
        instruction = null;
        if (!line.StartsWith("# if ", StringComparison.OrdinalIgnoreCase)) return false;

        var condition = line.Substring(5).Trim();
        instruction = new IfInstruction(condition, this.evaluator);
        return true;
    }
}

public class ElseParser : IInstructionParser {
    public bool TryParse(string line, out IMacroInstruction? instruction) {
        instruction = null;
        if (!line.Equals("# else", StringComparison.OrdinalIgnoreCase)) return false;

        instruction = new ElseInstruction();
        return true;
    }
}