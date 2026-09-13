namespace RolePlayer.Core.Emotes.Contracts;

public interface IUnlockSourceProvider {
    string GetUnlockSource(uint emoteId);
}