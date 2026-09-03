namespace RolePlayer.UI.EmoteBrowser.Models;

using System.Collections.Generic;

public class EmoteDisplayData {
    public uint Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public uint IconId { get; init; }
    public bool IsUnlockable { get; init; }
    public string UnlockRequirement { get; init; } = string.Empty;
    public string Category { get; init; } = string.Empty;
    public string LocalizedCommand { get; init; } = string.Empty;
    public string EnglishCommand { get; init; } = string.Empty;
    public bool IsUnlocked { get; set; }
    public bool IsModded { get; set; }
    public string ModName { get; set; } = string.Empty;
    public HashSet<string> CustomTags { get; set; } = new();
}