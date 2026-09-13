namespace RolePlayer.Core.GameEngine.Models;

public class DiceRollGameEvent : GameEvent {
    public int Roll { get; set; }
    public int MaxRoll { get; set; }
}