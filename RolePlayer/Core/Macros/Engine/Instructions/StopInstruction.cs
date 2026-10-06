namespace RolePlayer.Core.Macros.Engine.Instructions;

using RolePlayer.Core.Macros.Engine.Contracts;
using RolePlayer.Core.Macros.Engine.Models;

public class StopInstruction : IMacroInstruction {
    public void Execute(MacroExecutionContext context) {
        context.State = MacroExecutionState.Finished;
    }
}