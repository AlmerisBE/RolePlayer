namespace RolePlayer.Core.Macros.Services;

using RolePlayer.API.Interop.Contracts;
using RolePlayer.Core.Macros.Models;
using RolePlayer.UI.Hotbar.Contracts;

public class MacroExecutionService : IMacroExecutionService {
    private INativeExecutionService nativeExecution;

    public MacroExecutionService(INativeExecutionService nativeExecution) {
        this.nativeExecution = nativeExecution;
    }

    public void Execute(RoleplayMacro macro) {
        if (macro != null && !string.IsNullOrWhiteSpace(macro.Content)) {
            this.nativeExecution.Execute(macro.Content);
        }
    }
}