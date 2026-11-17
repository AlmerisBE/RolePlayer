namespace RolePlayer.API.GameEvents.Services;

using Dalamud.Game.ClientState.Objects.SubKinds;
using Dalamud.Plugin.Services;
using FFXIVClientStructs.FFXIV.Client.Game.Character;
using Lumina.Excel.Sheets;
using RolePlayer.API.GameEvents.Contracts;
using RolePlayer.Core.GameEngine.Contracts;
using RolePlayer.Core.GameEngine.Models;
using System;
using System.Collections.Generic;

public class EmoteWatcher : IGameEventWatcher {
    private IObjectTable objectTable;
    private IFramework framework;
    private IPlayerNameNormalizer nameNormalizer;
    private IDataManager dataManager;

    private HashSet<string> participants = new(StringComparer.OrdinalIgnoreCase);
    private Dictionary<nint, uint> previousEmotes = new();
    private bool isWatching;

    public event Action<GameEvent>? EventFired;
    public bool RestrictToParticipants { get; set; } = true;

    public EmoteWatcher(
        IObjectTable objectTable,
        IFramework framework,
        IPlayerNameNormalizer nameNormalizer,
        IDataManager dataManager) {

        this.objectTable = objectTable;
        this.framework = framework;
        this.nameNormalizer = nameNormalizer;
        this.dataManager = dataManager;
    }

    public void Start() {
        if (this.isWatching) return;
        this.previousEmotes.Clear();
        this.framework.Update += this.OnFrameworkUpdate;
        this.isWatching = true;
    }

    public void Stop() {
        if (!this.isWatching) return;
        this.framework.Update -= this.OnFrameworkUpdate;
        this.isWatching = false;
        this.previousEmotes.Clear();
    }

    public void SetParticipants(IEnumerable<string> participantNames) {
        this.participants = new HashSet<string>(participantNames, StringComparer.OrdinalIgnoreCase);
    }

    public void ClearParticipants() {
        this.participants.Clear();
    }

    private unsafe void OnFrameworkUpdate(IFramework fw) {
        var localPlayer = this.objectTable.LocalPlayer;
        if (localPlayer == null) return;

        uint localWorldId = localPlayer.HomeWorld.RowId;

        foreach (var obj in this.objectTable) {
            // Check specifically for Player Characters to access HomeWorld safely
            if (obj is not IPlayerCharacter chara) continue;

            string name = this.nameNormalizer.Normalize(chara.Name.TextValue);

            if (chara.HomeWorld.RowId != localWorldId && chara.HomeWorld.RowId != 0) {
                var worldSheet = this.dataManager.GetExcelSheet<World>();
                if (worldSheet != null) {
                    var world = worldSheet.GetRowOrDefault(chara.HomeWorld.RowId);
                    if (world.HasValue) name = $"{name}@{world.Value.Name.ToString()}";
                }
            }

            if (this.RestrictToParticipants && !this.participants.Contains(name)) continue;

            var ptr = (Character*)chara.Address;
            if (ptr == null) continue;

            uint currentEmote = ptr->EmoteController.EmoteId;
            this.previousEmotes.TryGetValue(chara.Address, out uint previousEmote);

            if (currentEmote != 0 && currentEmote != previousEmote) {
                this.EventFired?.Invoke(new EmoteGameEvent {
                    Sender = name,
                    EmoteId = currentEmote
                });
            }

            this.previousEmotes[chara.Address] = currentEmote;
        }
    }

    public void Dispose() {
        this.Stop();
    }
}