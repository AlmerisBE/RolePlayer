namespace RolePlayer.Core.Macros.Engine.Parsers;

using RolePlayer.Core.Macros.Engine.Contracts;
using RolePlayer.Core.Macros.Engine.Instructions;
using System;

public class CallMacroParser : IInstructionParser {
    private IServiceProvider serviceProvider;

    public CallMacroParser(IServiceProvider serviceProvider) {
        this.serviceProvider = serviceProvider;
    }

    public bool TryParse(string line, out IMacroInstruction? instruction) {
        instruction = null;
        if (!line.StartsWith("# call ", StringComparison.OrdinalIgnoreCase)) return false;

        var parts = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length >= 3 && int.TryParse(parts[2], out int commandId)) {
            instruction = new CallMacroInstruction(commandId, this.serviceProvider);
            return true;
        }

        return false;
    }
}