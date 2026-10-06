namespace RolePlayer.Core.Macros.Engine.Parsers;

using RolePlayer.Core.Macros.Engine.Contracts;
using RolePlayer.Core.Macros.Engine.Instructions;
using System;

public class StopParser : IInstructionParser {
    public bool TryParse(string line, out IMacroInstruction? instruction) {
        instruction = null;
        if (!line.Equals("# stop", StringComparison.OrdinalIgnoreCase)) return false;

        instruction = new StopInstruction();
        return true;
    }
}