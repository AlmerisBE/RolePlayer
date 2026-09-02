namespace RolePlayer.API.Penumbra.Contracts;

public interface IEmotePathProvider {
    string GetEmoteGamePath(uint emoteId);
}