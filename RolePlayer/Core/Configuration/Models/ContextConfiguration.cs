namespace RolePlayer.Core.Configuration.Models;

using RolePlayer.Core.MetaData.Models;
using RolePlayer.UI.Hotbar.Models;
using System;
using System.Collections.Generic;

public class ContextConfiguration {
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = "Default";
    public List<EmoteGroup> Groups { get; set; } = new();
    public List<string> Tags { get; set; } = new();
    public List<HotbarConfig> Hotbars { get; set; } = new();
}