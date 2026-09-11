namespace RolePlayer.Core.GameEngine.Models;

using System.Collections.Generic;

public class GameStage {
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string GmDescription { get; set; } = string.Empty;
    public List<GameActionConfig> OnEnterActions { get; set; } = new();
    public List<GameModuleConfig> ActiveModules { get; set; } = new();
    public List<GameTransition> Transitions { get; set; } = new();
}