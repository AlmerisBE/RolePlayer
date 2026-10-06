namespace RolePlayer.Core.Macros.Engine.Models;

using System;
using System.Collections.Generic;
using System.Numerics;

public class MacroExecutionContext {
    public Guid RootMacroId { get; set; }
    public Stack<MacroCallFrame> CallStack { get; } = new();
    public Dictionary<string, object> Variables { get; } = new(StringComparer.OrdinalIgnoreCase);
    public MacroExecutionState State { get; set; } = MacroExecutionState.Running;

    public DateTime ResumeTime { get; set; } = DateTime.MinValue;
    public bool CancelOnMove { get; set; }
    public Vector3 LastKnownPosition { get; set; }
    public DateTime LastMovementTime { get; set; } = DateTime.Now;

    public bool? LastConditionResult { get; set; }

    public bool YieldFrame { get; set; }
}