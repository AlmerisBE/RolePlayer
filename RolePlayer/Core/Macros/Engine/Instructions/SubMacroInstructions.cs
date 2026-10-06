namespace RolePlayer.Core.Macros.Engine.Instructions;

using Microsoft.Extensions.DependencyInjection;
using RolePlayer.Core.Macros.Contracts;
using RolePlayer.Core.Macros.Engine.Contracts;
using RolePlayer.Core.Macros.Engine.Models;
using System;

public class CallMacroInstruction : IMacroInstruction {
    private int commandId;
    private IServiceProvider serviceProvider;

    public CallMacroInstruction(int commandId, IServiceProvider serviceProvider) {
        this.commandId = commandId;
        this.serviceProvider = serviceProvider;
    }

    public void Execute(MacroExecutionContext context) {
        var macroService = this.serviceProvider.GetRequiredService<IMacroManagementService>();
        var compiler = this.serviceProvider.GetRequiredService<IMacroCompiler>();

        var macro = macroService.GetMacroByCommandId(this.commandId);

        if (macro != null) {
            if (compiler.TryCompile(macro, out var frame, out string error) && frame != null) {
                context.CallStack.Push(frame);
            }
            else {
                context.State = MacroExecutionState.Error;
            }
        }
    }
}