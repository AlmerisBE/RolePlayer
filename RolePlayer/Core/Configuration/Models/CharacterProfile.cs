namespace RolePlayer.Core.Configuration.Models;

using RolePlayer.Core.Macros.Models;
using System;
using System.Collections.Generic;

[Serializable]
public class CharacterProfile {
    public Dictionary<Guid, EmoteContext> Contexts { get; set; } = new();
    public Guid ActiveContextId { get; set; }
    public List<RoleplayMacro> Macros { get; set; } = new();
}