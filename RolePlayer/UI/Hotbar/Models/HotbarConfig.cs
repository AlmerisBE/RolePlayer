namespace RolePlayer.UI.Hotbar.Models;

using System;
using System.Collections.Generic;

[Serializable]
public class HotbarConfig {
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = "New Hotbar";
    public bool IsVisible { get; set; } = false;
    public HotbarLayout Layout { get; set; } = HotbarLayout.Grid16x1;
    public HotbarPopulationMode PopulationMode { get; set; } = HotbarPopulationMode.Manual;

    // Manual Population
    public List<uint> ManualEmoteIds { get; set; } = new();

    // Dynamic Population (Filters)
    public string SearchQuery { get; set; } = string.Empty;
    public HashSet<string> SelectedCategories { get; set; } = new();
    public HashSet<string> SelectedGroups { get; set; } = new();
    public HashSet<string> SelectedTags { get; set; } = new();
    public bool ShowModdedOnly { get; set; } = false;
}