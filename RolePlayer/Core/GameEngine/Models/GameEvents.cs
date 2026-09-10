namespace RolePlayer.Core.GameEngine.Models;

public abstract class GameEvent {
    public string Sender { get; init; } = string.Empty;
}

public class ChatGameEvent : GameEvent {
    public string Message { get; init; } = string.Empty;
    public GameChatChannel Channel { get; init; }
}

public class DiceRollGameEvent : GameEvent {
    public int Roll { get; init; }
    public int OutOf { get; init; }
}

public class EmoteGameEvent : GameEvent {
    public uint EmoteId { get; init; }
}