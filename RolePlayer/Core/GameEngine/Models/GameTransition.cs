namespace RolePlayer.Core.GameEngine.Models;

public class GameTransition {
    public string TargetStageId { get; set; } = string.Empty;
    public string TriggerType { get; set; } = "Manual";
    public string ConditionExpression { get; set; } = string.Empty;
}