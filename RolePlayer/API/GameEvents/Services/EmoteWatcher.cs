namespace RolePlayer.API.GameEvents.Services;

using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Plugin.Services;
using FFXIVClientStructs.FFXIV.Client.Game.Character;
using RolePlayer.Core.GameEngine.Contracts;
using RolePlayer.Core.GameEngine.Models;
using System;
using System.Collections.Generic;

public class EmoteWatcher : IGameEventWatcher {
    private IObjectTable objectTable;
    private IFramework framework;

    private HashSet<string> participants = new(StringComparer.OrdinalIgnoreCase);
    private Dictionary<nint, uint> previousEmotes = new();
    private bool isWatching;

    public event Action<GameEvent>? EventFired;

    public EmoteWatcher(IObjectTable objectTable, IFramework framework) {
        this.objectTable = objectTable;
        this.framework = framework;
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
        foreach (var obj in this.objectTable) {
            if (obj is not ICharacter chara) continue;

            string name = chara.Name.TextValue;
            if (this.participants.Count > 0 && !this.participants.Contains(name)) continue;

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