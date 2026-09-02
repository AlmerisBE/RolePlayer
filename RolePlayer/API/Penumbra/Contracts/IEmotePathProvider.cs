namespace RolePlayer.API.Penumbra.Contracts;

using System.Collections.Generic;

public interface IEmotePathProvider {
    IEnumerable<string> GetEmoteGamePaths(uint emoteId);
}