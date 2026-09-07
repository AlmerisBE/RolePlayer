namespace RolePlayer.Core.Configuration.Models;

using RolePlayer.Core.Macros.Models;
using System;
using System.Collections.Generic;

public class CharacterProfile {
    public ulong CharacterId { get; set; }
    public string CharacterName { get; set; } = string.Empty;
    public Guid ActiveContextId { get; set; } = Guid.Empty;
    public List<RoleplayMacro> Macros { get; set; } = new();
}