namespace RolePlayer.Core.Macros.Engine.Contracts;

public interface IInstructionParser {
    bool TryParse(string line, out IMacroInstruction? instruction);
}