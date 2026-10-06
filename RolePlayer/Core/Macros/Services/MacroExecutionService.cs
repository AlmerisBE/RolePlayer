namespace RolePlayer.Core.Macros.Services;

using RolePlayer.Core.Macros.Contracts;
using RolePlayer.Core.Macros.Engine.Contracts;
using RolePlayer.Core.Macros.Models;

public class MacroExecutionService : IMacroExecutionService {
    private IMacroEngine engine;

    public MacroExecutionService(IMacroEngine engine) {
        this.engine = engine;
    }

    public void Execute(RoleplayMacro macro) {
        if (macro != null) this.engine.Play(macro);
    }
}