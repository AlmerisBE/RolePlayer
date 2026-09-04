namespace RolePlayer.Core.Configuration.Models;

using RolePlayer.Core.MetaData.Models;
using RolePlayer.UI.EmoteBrowser.Models;
using RolePlayer.UI.Hotbar.Models;
using System;
using System.Collections.Generic;

[Serializable]
public class EmoteContext {
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = "Default";

    public List<EmoteGroup> EmoteGroups { get; set; } = new();
    public Dictionary<uint, HashSet<string>> EmoteTags { get; set; } = new();
    public Dictionary<uint, string> EmoteToGroupMap { get; set; } = new();
    public HashSet<string> AvailableTags { get; set; } = new();

    public bool ShowFilters { get; set; } = false;
    public bool ShowModdedOnly { get; set; } = false;
    public UnlockFilterMode UnlockFilter { get; set; } = UnlockFilterMode.All;
    public GroupingMode CurrentGrouping { get; set; } = GroupingMode.NativeCategory;
    public HashSet<string> SelectedCategories { get; set; } = new();
    public HashSet<string> SelectedGroups { get; set; } = new();
    public HashSet<string> SelectedTags { get; set; } = new();

    public List<HotbarConfig> Hotbars { get; set; } = new();
}