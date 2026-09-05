namespace RolePlayer.UI.Themes.Models;

using System.Collections.Generic;

public class RolePlayerTheme {
    public string Name { get; set; } = string.Empty;
    public string Author { get; set; } = string.Empty;
    public Dictionary<string, string> Palette { get; set; } = new();
}