namespace RolePlayer.Core.GameEngine.Models;

using System;
using System.Collections.Generic;

public class GameActionConfig {
    public string ActionType { get; set; } = string.Empty;
    public Dictionary<string, string> Parameters { get; set; } = new(StringComparer.OrdinalIgnoreCase);
}