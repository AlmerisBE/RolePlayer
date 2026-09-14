namespace RolePlayer.Core.GameEngine.Models;

using System;
using System.Collections.Generic;

public class GameModuleConfig {
    public string ModuleType { get; set; } = string.Empty;
    public Dictionary<string, string> Parameters { get; set; } = new(StringComparer.OrdinalIgnoreCase);
    public List<string> ConditionExpressions { get; set; } = new();
    public List<GameActionConfig> OnTriggerActions { get; set; } = new();
}