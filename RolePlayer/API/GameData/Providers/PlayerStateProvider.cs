namespace RolePlayer.API.GameData.Providers;

using Dalamud.Game.ClientState.Conditions;
using Dalamud.Plugin.Services;
using FFXIVClientStructs.FFXIV.Client.Game.Character;
using FFXIVClientStructs.FFXIV.Client.Game.UI;
using RolePlayer.UI.EmoteBrowser.Contracts;
using System;

public class PlayerStateProvider : IPlayerStateProvider {
    private IObjectTable objectTable;
    private IFramework framework;
    private ICondition condition;

    private bool isPlayerValid = false;
    private bool wasPlayerValid = false;

    public bool IsPlayerValid => this.isPlayerValid;
    public event Action? PlayerStateValid;

    public PlayerStateProvider(IObjectTable objectTable, IFramework framework, ICondition condition) {
        this.objectTable = objectTable;
        this.framework = framework;
        this.condition = condition;

        this.framework.Update += this.OnFrameworkUpdate;
    }

    private void OnFrameworkUpdate(IFramework fw) {
        bool isLoading = this.condition[ConditionFlag.BetweenAreas] || this.condition[ConditionFlag.BetweenAreas51];
        this.isPlayerValid = this.objectTable.LocalPlayer != null && !isLoading;

        if (this.isPlayerValid && !this.wasPlayerValid) {
            this.PlayerStateValid?.Invoke();
        }

        this.wasPlayerValid = this.isPlayerValid;
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