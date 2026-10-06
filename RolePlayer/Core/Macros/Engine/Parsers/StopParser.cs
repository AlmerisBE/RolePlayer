namespace RolePlayer.Core.Macros.Engine.Parsers;

using RolePlayer.Core.Macros.Engine.Contracts;
using RolePlayer.Core.Macros.Engine.Instructions;
using System;

public class StopParser : IInstructionParser {
    public bool TryParse(string line, out IMacroInstruction? instruction, out string errorMessage) {
        instruction = null;
        errorMessage = string.Empty;
        if (!line.Equals("# stop", StringComparison.OrdinalIgnoreCase)) return false;

        instruction = new StopInstruction();
        return true;
    }
}