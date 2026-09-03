namespace RolePlayer.API.GameData.Providers;

using Dalamud.Plugin.Services;
using FFXIVClientStructs.FFXIV.Client.Game.UI;
using RolePlayer.UI.EmoteBrowser.Contracts;
using System;

public class PlayerStateProvider : IPlayerStateProvider, IDisposable {
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

    public void Dispose() {
        this.framework.Update -= this.OnFrameworkUpdate;
    }
}