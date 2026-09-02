namespace RolePlayer.UI.EmoteBrowser.Tabs;

using Dalamud.Bindings.ImGui;
using RolePlayer.UI.EmoteBrowser.Contracts;
using RolePlayer.UI.EmoteBrowser.Models;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

public class AllEmotesTab : IEmoteBrowserTab {
    private IEmoteRepository emoteRepository;
    private IPlayerStateProvider playerStateProvider;
    private IEmoteSelectionState selectionState;
    private IModStateProvider modStateProvider;
    private List<EmoteDisplayData> emotesCache;

    public string TabName => "All Emotes";

    public AllEmotesTab(
        IEmoteRepository emoteRepository,
        IPlayerStateProvider playerStateProvider,
        IEmoteSelectionState selectionState,
        IModStateProvider modStateProvider) { // Injection du fournisseur d'état des mods

        this.emoteRepository = emoteRepository;
        this.playerStateProvider = playerStateProvider;
        this.selectionState = selectionState;
        this.modStateProvider = modStateProvider;
        this.emotesCache = new List<EmoteDisplayData>();
    }

    public void Draw() {
        if (!this.emotesCache.Any()) {
            this.LoadEmotes();
        }

        if (ImGui.BeginChild("AllEmotesList")) {
            foreach (var emote in this.emotesCache) {
                var isSelected = this.selectionState.SelectedEmote?.Id == emote.Id;
                var hasCustomColor = false;

                // Application des couleurs : grisé si verrouillé, vert si moddé et débloqué
                if (!emote.IsUnlocked) {
                    ImGui.PushStyleColor(ImGuiCol.Text, 0xFF808080);
                    hasCustomColor = true;
                }
                else if (emote.IsModded) {
                    ImGui.PushStyleColor(ImGuiCol.Text, new Vector4(0.2f, 0.8f, 0.2f, 1.0f));
                    hasCustomColor = true;
                }

                // Ajout d'un signe distinctif pour les emotes modifiées
                var displayName = emote.IsModded ? $"★ {emote.Name}" : emote.Name;

                if (ImGui.Selectable(displayName, isSelected)) {
                    this.selectionState.SelectedEmote = emote;
                }

                if (hasCustomColor) {
                    ImGui.PopStyleColor();
                }
            }
            ImGui.EndChild();
        }
    }

    private void LoadEmotes() {
        var baseEmotes = this.emoteRepository.GetBaseEmotes();
        foreach (var emote in baseEmotes) {
            emote.IsUnlocked = !emote.IsUnlockable || this.playerStateProvider.IsEmoteUnlocked(emote.Id);

            // Vérification de la modification via Penumbra IPC
            var modName = this.modStateProvider.GetModNameModifyingEmote(emote.Id);
            emote.IsModded = !string.IsNullOrEmpty(modName);

            this.emotesCache.Add(emote);
        }
    }
}