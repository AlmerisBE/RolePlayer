namespace RolePlayer.UI.EmoteBrowser.Contracts;

public interface IEmoteExecutionService {
    void ExecuteEmote(uint emoteId);
    void OpenNativeEmoteWindow();
}