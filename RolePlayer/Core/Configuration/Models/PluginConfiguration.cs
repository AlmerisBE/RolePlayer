namespace RolePlayer.Core.Configuration.Models;

using Dalamud.Configuration;
using RolePlayer.Core.MetaData.Models;
using System;
using System.Collections.Generic;

[Serializable]
public class PluginConfiguration : IPluginConfiguration {
    public int Version { get; set; } = 0;

    // Custom Emote Groups
    public List<EmoteGroup> EmoteGroups { get; set; } = new();

    // EmoteId -> HashSet of Custom Tags
    public Dictionary<uint, HashSet<string>> EmoteTags { get; set; } = new();
}