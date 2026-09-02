namespace RolePlayer.UI.EmoteBrowser.Tabs;

using Dalamud.Bindings.ImGui;
using RolePlayer.UI.EmoteBrowser.Contracts;
using RolePlayer.UI.EmoteBrowser.Models;
using System.Collections.Generic;
using System.Linq;

public class AllEmotesTab : IEmoteBrowserTab {
    private IEmoteRepository emoteRepository;
    private IPlayerStateProvider playerStateProvider;
    private IEmoteSelectionState selectionState;
    private List<EmoteDisplayData> emotesCache;

    public string TabName => "All Emotes";

    public AllEmotesTab(
        IEmoteRepository emoteRepository,
        IPlayerStateProvider playerStateProvider,
        IEmoteSelectionState selectionState) {
        this.emoteRepository = emoteRepository;
        this.playerStateProvider = playerStateProvider;
        this.selectionState = selectionState;
        this.emotesCache = new List<EmoteDisplayData>();
    }

    public void Draw() {
        if (!this.emotesCache.Any()) {
            this.LoadEmotes();
        }

        if (ImGui.BeginChild("AllEmotesList")) {
            foreach (var emote in this.emotesCache) {
                var isSelected = this.selectionState.SelectedEmote?.Id == emote.Id;

                // Griser le texte si l'emote n'est pas débloquée
                if (!emote.IsUnlocked) {
                    ImGui.PushStyleColor(ImGuiCol.Text, 0xFF808080);
                }

                if (ImGui.Selectable(emote.Name, isSelected)) {
                    this.selectionState.SelectedEmote = emote;
                }

                if (!emote.IsUnlocked) {
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
            this.emotesCache.Add(emote);
        }
    }
}