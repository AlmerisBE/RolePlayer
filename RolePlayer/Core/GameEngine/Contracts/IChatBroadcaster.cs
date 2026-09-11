namespace RolePlayer.Core.GameEngine.Contracts;

using RolePlayer.Core.GameEngine.Models;

public interface IChatBroadcaster {
    void Broadcast(string message, GameChatChannel channel);
}