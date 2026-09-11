namespace RolePlayer.Core.GameEngine.Models;

using System;
using System.Collections.Generic;

public class GameDefinition {
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public string Author { get; set; } = string.Empty;
    public string Version { get; set; } = "1.0.0";
    public string Description { get; set; } = string.Empty;
    public string EngineType { get; set; } = "Generic";

    public bool AllowChatRegistration { get; set; } = false;
    public List<string> Stages { get; set; } = new();
    public Dictionary<string, string> Parameters { get; set; } = new();
    public Dictionary<string, string> Messages { get; set; } = new();
}