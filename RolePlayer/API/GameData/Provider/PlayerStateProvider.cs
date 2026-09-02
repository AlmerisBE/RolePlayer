namespace RolePlayer.API.GameData.Providers;

using Dalamud.Plugin.Services;
using FFXIVClientStructs.FFXIV.Client.Game.UI;
using RolePlayer.UI.EmoteBrowser.Contracts;

public class PlayerStateProvider : IPlayerStateProvider {
    private IObjectTable objectTable;

    public PlayerStateProvider(IObjectTable objectTable) {
        this.objectTable = objectTable;
    }

    public unsafe bool IsEmoteUnlocked(uint emoteId) {
        // Safe check using LocalPlayer via IObjectTable as per Dawntrail guidelines
        if (this.objectTable.LocalPlayer == null) {
            return false;
        }

        var uiState = UIState.Instance();
        if (uiState == null) {
            return false;
        }

        return uiState->IsEmoteUnlocked((ushort)emoteId);
    }
}