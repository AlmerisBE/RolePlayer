namespace RolePlayer.API.GameEvents.Extensions;

using Dalamud.Game.Text;
using RolePlayer.Core.GameEngine.Models;

public static class XivChatTypeExtensions {
    public static GameChatChannel? ToGameChatChannel(this XivChatType type) {
        return type switch {
            XivChatType.Say => GameChatChannel.Say,
            XivChatType.Yell => GameChatChannel.Yell,
            XivChatType.Shout => GameChatChannel.Shout,
            XivChatType.Party => GameChatChannel.Party,
            XivChatType.Alliance => GameChatChannel.Alliance,
            XivChatType.FreeCompany => GameChatChannel.FreeCompany,
            _ => null
        };
    }
}