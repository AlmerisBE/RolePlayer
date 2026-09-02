namespace RolePlayer.API.GameData.Providers;

using FFXIVClientStructs.FFXIV.Client.Game.UI;
using RolePlayer.UI.EmoteBrowser.Contracts;

public class PlayerStateProvider : IPlayerStateProvider {
    public unsafe bool IsEmoteUnlocked(uint emoteId) {
        var uiState = UIState.Instance();
        if (uiState == null) {
            return false;
        }

        // FFXIVClientStructs expects a ushort for emote IDs, while Lumina exposes uint RowIds
        return uiState->IsEmoteUnlocked((ushort)emoteId);
    }
}