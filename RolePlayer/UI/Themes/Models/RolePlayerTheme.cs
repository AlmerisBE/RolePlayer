namespace RolePlayer.UI.Themes.Models;

using System.Collections.Generic;

public class RolePlayerTheme {
    public string Name { get; set; } = string.Empty;
    public Dictionary<string, string> Colors { get; set; } = new();
}