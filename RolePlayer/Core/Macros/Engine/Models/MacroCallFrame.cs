namespace RolePlayer.Core.Macros.Engine.Models;

using RolePlayer.Core.Macros.Engine.Contracts;
using System;
using System.Collections.Generic;

public class MacroCallFrame {
    public Guid MacroId { get; set; }
    public IReadOnlyList<IMacroInstruction> Instructions { get; set; } = Array.Empty<IMacroInstruction>();
    public Dictionary<string, int> Labels { get; set; } = new(StringComparer.OrdinalIgnoreCase);
    public int ProgramCounter { get; set; } = 0;
}