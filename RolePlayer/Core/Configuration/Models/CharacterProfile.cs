namespace RolePlayer.Core.Configuration.Models;

using System;
using System.Collections.Generic;

[Serializable]
public class CharacterProfile {
    public Dictionary<Guid, EmoteContext> Contexts { get; set; } = new();
    public Guid ActiveContextId { get; set; }
}