namespace RolePlayer.Core.Emotes.Contracts;

public interface IEmoteExecutionService {
    void ExecuteEmote(uint emoteId);
    void OpenNativeEmoteWindow();
}