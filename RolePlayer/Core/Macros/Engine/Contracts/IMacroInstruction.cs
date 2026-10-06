using RolePlayer.Core.Macros.Engine.Models;

namespace RolePlayer.Core.Macros.Engine.Contracts;

public interface IMacroInstruction {
    void Execute(MacroExecutionContext context);
}