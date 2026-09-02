namespace RolePlayer.UI.EmoteBrowser.Tabs;

using Dalamud.Bindings.ImGui;
using Dalamud.Game;
using Dalamud.Interface.Textures;
using Dalamud.Interface.Textures.Internal;
using Dalamud.Plugin.Services;
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
    private IEmoteExecutionService executionService;
    private ITextureProvider textureProvider;
    private IClientState clientState;

    private List<EmoteDisplayData> emotesCache;

    public string TabName => "All Emotes";

    public AllEmotesTab(
        IEmoteRepository emoteRepository,
        IPlayerStateProvider playerStateProvider,
        IEmoteSelectionState selectionState,
        IModStateProvider modStateProvider,
        IEmoteExecutionService executionService,
        ITextureProvider textureProvider,
        IClientState clientState) {

        this.emoteRepository = emoteRepository;
        this.playerStateProvider = playerStateProvider;
        this.selectionState = selectionState;
        this.modStateProvider = modStateProvider;
        this.executionService = executionService;
        this.textureProvider = textureProvider;
        this.clientState = clientState;

        this.emotesCache = new List<EmoteDisplayData>();
    }

    public void Draw() {
        if (!this.emotesCache.Any()) {
            this.LoadEmotes();
        }

        var tableFlags = ImGuiTableFlags.BordersInnerH | ImGuiTableFlags.RowBg | ImGuiTableFlags.ScrollY;

        if (ImGui.BeginTable("AllEmotesTable", 4, tableFlags)) {
            ImGui.TableSetupColumn("Icon", ImGuiTableColumnFlags.WidthFixed, 32f);
            ImGui.TableSetupColumn("Name", ImGuiTableColumnFlags.WidthStretch, 0.4f);
            ImGui.TableSetupColumn("Command", ImGuiTableColumnFlags.WidthStretch, 0.4f);
            ImGui.TableSetupColumn("Action", ImGuiTableColumnFlags.WidthFixed, 60f);
            ImGui.TableHeadersRow();

            foreach (var emote in this.emotesCache) {
                ImGui.TableNextRow();

                var isSelected = this.selectionState.SelectedEmote?.Id == emote.Id;
                var hasCustomColor = false;

                if (!emote.IsUnlocked) {
                    ImGui.PushStyleColor(ImGuiCol.Text, 0xFF808080);
                    hasCustomColor = true;
                }
                else if (emote.IsModded) {
                    ImGui.PushStyleColor(ImGuiCol.Text, new Vector4(0.2f, 0.8f, 0.2f, 1.0f));
                    hasCustomColor = true;
                }

                // Column 1: Icon + Global selectable
                ImGui.TableNextColumn();
                if (ImGui.Selectable($"##select_{emote.Id}", isSelected, ImGuiSelectableFlags.SpanAllColumns | ImGuiSelectableFlags.AllowItemOverlap)) {
                    this.selectionState.SelectedEmote = emote;
                }

                ImGui.SameLine();

                if (emote.IconId > 0) {
                    try {
                        // Explicitly disable HiRes lookup to prevent IconNotFoundException on standard emote icons
                        var lookup = new GameIconLookup {
                            IconId = emote.IconId,
                            HiRes = false
                        };

                        var iconWrap = this.textureProvider.GetFromGameIcon(lookup).GetWrapOrDefault();
                        if (iconWrap != null) {
                            ImGui.Image(iconWrap.Handle, new Vector2(24, 24));
                        }
                    }
                    catch (IconNotFoundException) {
                        // Gracefully ignore missing textures to maintain UI stability
                    }
                }

                // Column 2: Name
                ImGui.TableNextColumn();
                var displayName = emote.IsModded ? $"★ {emote.Name}" : emote.Name;
                ImGui.AlignTextToFramePadding();
                ImGui.Text(displayName);

                // Column 3: Command(s)
                ImGui.TableNextColumn();
                ImGui.AlignTextToFramePadding();
                var commandText = emote.LocalizedCommand;

                if (this.clientState.ClientLanguage != ClientLanguage.English && !string.IsNullOrEmpty(emote.EnglishCommand) && emote.EnglishCommand != emote.LocalizedCommand) {
                    commandText += $" / {emote.EnglishCommand}";
                }

                ImGui.Text(commandText);

                // Column 4: Quick Play
                ImGui.TableNextColumn();
                if (emote.IsUnlocked) {
                    if (ImGui.Button($"Play##{emote.Id}", new Vector2(-1, 24))) {
                        this.executionService.ExecuteEmote(emote.Id);
                    }
                }

                if (hasCustomColor) {
                    ImGui.PopStyleColor();
                }
            }
            ImGui.EndTable();
        }
    }

    private void LoadEmotes() {
        var baseEmotes = this.emoteRepository.GetBaseEmotes();
        foreach (var emote in baseEmotes) {
            emote.IsUnlocked = !emote.IsUnlockable || this.playerStateProvider.IsEmoteUnlocked(emote.Id);
            var modName = this.modStateProvider.GetModNameModifyingEmote(emote.Id);
            emote.IsModded = !string.IsNullOrEmpty(modName);
            this.emotesCache.Add(emote);
        }
    }
}