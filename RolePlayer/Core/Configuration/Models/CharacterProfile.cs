namespace RolePlayer.Core.Configuration.Models;

using System;
using System.Collections.Generic;

[Serializable]
public class CharacterProfile {
    public Dictionary<Guid, EmoteContext> Contexts { get; set; } = new();
    public Guid ActiveContextId { get; set; }

    public CharacterProfile() {
        var defaultContext = new EmoteContext { Name = "Default" };
        this.Contexts.Add(defaultContext.Id, defaultContext);
        this.ActiveContextId = defaultContext.Id;
    }
}