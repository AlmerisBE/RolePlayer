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
    public Dictionary<uint, string> EmoteToGroupMap { get; set; } = new();

    // Liste globale des tags créés par l'utilisateur
    public HashSet<string> AvailableTags { get; set; } = new();
}