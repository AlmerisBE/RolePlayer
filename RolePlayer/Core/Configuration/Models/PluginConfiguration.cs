namespace RolePlayer.Core.Configuration.Models;

using Dalamud.Configuration;
using RolePlayer.Core.MetaData.Models;
using System;
using System.Collections.Generic;

[Serializable]
public class PluginConfiguration : IPluginConfiguration {
    public int Version { get; set; } = 0;

    public List<EmoteGroup> EmoteGroups { get; set; } = new();
    public Dictionary<uint, HashSet<string>> EmoteTags { get; set; } = new();

    // Mapping entre l'ID d'une emote et le nom de son groupe assigné
    public Dictionary<uint, string> EmoteToGroupMap { get; set; } = new();
}