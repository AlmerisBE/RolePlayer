namespace RolePlayer.UI.EmoteBrowser.Contracts;

public interface IUnlockSourceProvider {
    string GetUnlockSource(uint emoteId);
}