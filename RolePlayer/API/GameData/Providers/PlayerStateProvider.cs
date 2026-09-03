namespace RolePlayer.API.GameData.Providers;

using Dalamud.Plugin.Services;
using FFXIVClientStructs.FFXIV.Client.Game.Character;
using FFXIVClientStructs.FFXIV.Client.Game.UI;
using RolePlayer.UI.EmoteBrowser.Contracts;

public class PlayerStateProvider : IPlayerStateProvider {
    private IObjectTable objectTable;
    private IFramework framework;

    private bool isPlayerValid = false;

    public PlayerStateProvider(IObjectTable objectTable, IFramework framework) {
        this.objectTable = objectTable;
        this.framework = framework;

        this.framework.Update += this.OnFrameworkUpdate;
    }

    private void OnFrameworkUpdate(IFramework fw) {
        this.isPlayerValid = this.objectTable.LocalPlayer != null;
    }

    public unsafe bool IsEmoteUnlocked(uint emoteId) {
        if (!this.isPlayerValid) {
            return false;
        }

        var uiState = UIState.Instance();
        if (uiState == null) {
            return false;
        }

        return uiState->IsEmoteUnlocked((ushort)emoteId);
    }

    public unsafe bool IsEmoteActive(uint emoteId) {
        var localPlayer = this.objectTable.LocalPlayer;
        if (localPlayer == null) {
            return false;
        }

        var character = (Character*)localPlayer.Address;
        if (character == null) {
            return false;
        }

        return character->EmoteController.EmoteId == emoteId;
    }

    public unsafe uint GetActiveEmoteId() {
        var localPlayer = this.objectTable.LocalPlayer;
        if (localPlayer == null) {
            return 0;
        }

        var character = (Character*)localPlayer.Address;
        if (character == null) {
            return 0;
        }

        return character->EmoteController.EmoteId;
    }

    public void Dispose() {
        this.framework.Update -= this.OnFrameworkUpdate;
    }
}