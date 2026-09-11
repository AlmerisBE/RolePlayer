namespace RolePlayer.Core.GameEngine.Models;

using System;
using System.Collections.Generic;

public class GameSessionContext {
    public Dictionary<string, object> Variables { get; } = new(StringComparer.OrdinalIgnoreCase);
    public HashSet<string> Participants { get; } = new(StringComparer.OrdinalIgnoreCase);
    public GameEvent? CurrentEvent { get; set; }
}