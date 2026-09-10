namespace RolePlayer.Core.GameHost.Models;

using System.Collections.Generic;

public class GameSessionConfig {
    public GameDefinition? Game { get; set; }
    public HashSet<GameChatChannel> ListeningChannels { get; set; } = new();
    public Dictionary<string, string> ActiveParameters { get; set; } = new();
}