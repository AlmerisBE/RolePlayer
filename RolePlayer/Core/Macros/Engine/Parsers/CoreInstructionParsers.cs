namespace RolePlayer.Core.Macros.Engine.Parsers;

using RolePlayer.Core.Macros.Engine.Contracts;
using RolePlayer.Core.Macros.Engine.Instructions;
using System;
using System.Globalization;

public class WaitParser : IInstructionParser {
    public bool TryParse(string line, out IMacroInstruction? instruction, out string errorMessage) {
        instruction = null;
        errorMessage = string.Empty;
        if (!line.StartsWith("# wait ", StringComparison.OrdinalIgnoreCase)) return false;

        var parts = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length >= 3 && float.TryParse(parts[2], NumberStyles.Any, CultureInfo.InvariantCulture, out float delay)) instruction = new WaitInstruction(delay);
        else errorMessage = "Invalid syntax. Expected: # wait <seconds>";

        return true;
    }
}

public class LabelParser : IInstructionParser {
    public bool TryParse(string line, out IMacroInstruction? instruction, out string errorMessage) {
        instruction = null;
        errorMessage = string.Empty;
        if (!line.StartsWith("# label ", StringComparison.OrdinalIgnoreCase)) return false;

        var parts = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length >= 3) instruction = new LabelInstruction(parts[2]);
        else errorMessage = "Invalid syntax. Expected: # label <Name>";

        return true;
    }
}

public class GotoParser : IInstructionParser {
    public bool TryParse(string line, out IMacroInstruction? instruction, out string errorMessage) {
        instruction = null;
        errorMessage = string.Empty;
        if (!line.StartsWith("# goto ", StringComparison.OrdinalIgnoreCase)) return false;

        var parts = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length >= 3) instruction = new GotoInstruction(parts[2]);
        else errorMessage = "Invalid syntax. Expected: # goto <Name>";

        return true;
    }
}

public class SetVariableParser : IInstructionParser {
    public bool TryParse(string line, out IMacroInstruction? instruction, out string errorMessage) {
        instruction = null;
        errorMessage = string.Empty;
        if (!line.StartsWith("# set ", StringComparison.OrdinalIgnoreCase)) return false;

        var parts = line.Split(' ', 4, StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length >= 4) {
            var varName = parts[2].Trim('{', '}');
            instruction = new SetVariableInstruction(varName, parts[3]);
        }
        else {
            errorMessage = "Invalid syntax. Expected: # set {VariableName} <Value>";
        }

        return true;
    }
}