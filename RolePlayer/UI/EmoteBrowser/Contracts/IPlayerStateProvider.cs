namespace RolePlayer.UI.EmoteBrowser.Contracts;

public interface IPlayerStateProvider {
    bool IsEmoteUnlocked(uint emoteId);
    bool IsEmoteActive(uint emoteId);
}