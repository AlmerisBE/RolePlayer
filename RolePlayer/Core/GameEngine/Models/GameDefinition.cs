namespace RolePlayer.Core.GameEngine.Models;

using System;
using System.Collections.Generic;

public class GameDefinition {
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public string Author { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;

    // Propriétés héritées pour le Dashboard
    public bool AllowChatRegistration { get; set; } = false;
    public Dictionary<string, string> Messages { get; set; } = new(StringComparer.OrdinalIgnoreCase);
    public Dictionary<string, string> Parameters { get; set; } = new(StringComparer.OrdinalIgnoreCase);

    public List<GameVariableDefinition> ExposedVariables { get; set; } = new();
    public Dictionary<string, object> InitialVariables { get; set; } = new(StringComparer.OrdinalIgnoreCase);
    public List<GameStage> Stages { get; set; } = new();
}