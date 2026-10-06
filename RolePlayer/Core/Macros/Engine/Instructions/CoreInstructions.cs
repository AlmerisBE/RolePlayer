namespace RolePlayer.Core.Macros.Engine.Instructions;

using RolePlayer.API.Interop.Contracts;
using RolePlayer.Core.Macros.Engine.Contracts;
using RolePlayer.Core.Macros.Engine.Models;
using System;

public class NativeCommandInstruction : IMacroInstruction {
    private string rawCommand;
    private INativeExecutionService nativeExecution;

    public NativeCommandInstruction(string rawCommand, INativeExecutionService nativeExecution) {
        this.rawCommand = rawCommand;
        this.nativeExecution = nativeExecution;
    }

    public void Execute(MacroExecutionContext context) {
        string formatted = this.rawCommand;
        foreach (var kvp in context.Variables) {
            formatted = formatted.Replace($"{{{kvp.Key}}}", kvp.Value?.ToString() ?? string.Empty, StringComparison.OrdinalIgnoreCase);
        }
        this.nativeExecution.Execute(formatted);
    }
}

public class WaitInstruction : IMacroInstruction {
    private float delaySeconds;
    public WaitInstruction(float delaySeconds) => this.delaySeconds = delaySeconds;
    public void Execute(MacroExecutionContext context) => context.ResumeTime = DateTime.Now.AddSeconds(this.delaySeconds);
}

public class LabelInstruction : IMacroInstruction {
    public string LabelName { get; }
    public LabelInstruction(string labelName) => this.LabelName = labelName;
    public void Execute(MacroExecutionContext context) { }
}

public class GotoInstruction : IMacroInstruction {
    private string labelName;
    public GotoInstruction(string labelName) => this.labelName = labelName;

    public void Execute(MacroExecutionContext context) {
        var frame = context.CallStack.Peek();
        if (frame.Labels.TryGetValue(this.labelName, out int index)) frame.ProgramCounter = index;
        else context.State = MacroExecutionState.Error;
    }
}

public class SetVariableInstruction : IMacroInstruction {
    private string targetVar;
    private string value;

    public SetVariableInstruction(string targetVar, string value) {
        this.targetVar = targetVar;
        this.value = value;
    }

    public void Execute(MacroExecutionContext context) {
        context.Variables[this.targetVar] = this.value;
    }
}