namespace RolePlayer.UI.Hotbar.Models;

using RolePlayer.Core.Macros.Models;
using System;

public class ResolvedHotbarItem {
    public uint? EmoteId { get; set; }
    public Guid? MacroId { get; set; }
    public RoleplayMacro? MacroReference { get; set; }

    public string Name { get; set; } = string.Empty;
    public uint IconId { get; set; }
    public bool HasVariations { get; set; }
    public bool IsModded { get; set; }
    public string ModName { get; set; } = string.Empty;
    public string CommandText { get; set; } = string.Empty;
}