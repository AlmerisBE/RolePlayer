namespace RolePlayer.Core.Macros.Models;

using System;

public class RoleplayMacro {
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = "New Macro";
    public string Content { get; set; } = string.Empty;
    public uint IconId { get; set; }
}