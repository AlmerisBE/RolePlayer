namespace RolePlayer.Core.Configuration.Models;

using Dalamud.Configuration;
using System;
using System.Collections.Generic;

[Serializable]
public class PluginConfiguration : IPluginConfiguration {
    public int Version { get; set; } = 1;
    public Dictionary<string, CharacterProfile> Profiles { get; set; } = new();
}