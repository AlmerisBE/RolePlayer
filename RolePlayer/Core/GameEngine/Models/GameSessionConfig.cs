namespace RolePlayer.Core.GameEngine.Models;

using System.Collections.Generic;

public class GameSessionConfig {
    public GameDefinition? Game { get; set; }
    public GameChatChannel BroadcastChannel { get; set; } = GameChatChannel.Say;
    public HashSet<GameChatChannel> ListeningChannels { get; set; } = new();
    public Dictionary<string, string> ActiveParameters { get; set; } = new();
}