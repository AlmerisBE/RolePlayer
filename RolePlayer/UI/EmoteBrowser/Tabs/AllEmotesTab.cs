namespace RolePlayer.UI.EmoteBrowser.Tabs;

using Dalamud.Bindings.ImGui;
using Dalamud.Interface;
using Dalamud.Interface.Textures;
using Dalamud.Plugin.Services;
using RolePlayer.UI.EmoteBrowser.Contracts;
using RolePlayer.UI.EmoteBrowser.Models;
using RolePlayer.UI.Hotbar.Contracts;
using RolePlayer.UI.Localization.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

public class AllEmotesTab : IEmoteBrowserTab, IDisposable {
    private IEmoteRepository emoteRepository;
    private IEmoteSelectionState selectionState;
    private IEmoteExecutionService executionService;
    private ILocalizationService localization;
    private IMacroManagementService macroService;
    private ITextureProvider textureProvider;

    private string searchQuery = string.Empty;
    private bool hideEmotesWithoutCommand = true;
    private List<EmoteDisplayData> cachedEmotes = new();

    public string TabName => this.localization.Translate("browser_tab_all_emotes");
    public int SortOrder => 10;
    public bool IsSidePanelOpen => this.selectionState.SelectedEmote != null;

    public AllEmotesTab(
        IEmoteRepository emoteRepository,
        IEmoteSelectionState selectionState,
        IEmoteExecutionService executionService,
        ILocalizationService localization,
        IMacroManagementService macroService,
        ITextureProvider textureProvider) {

        this.emoteRepository = emoteRepository;
        this.selectionState = selectionState;
        this.executionService = executionService;
        this.localization = localization;
        this.macroService = macroService;
        this.textureProvider = textureProvider;

        this.cachedEmotes = this.emoteRepository.GetBaseEmotes().ToList();
    }

    public void Draw() {
        ImGui.SetNextItemWidth(-1f);
        ImGui.InputTextWithHint("##EmoteSearch", this.localization.Translate("browser_search_hint"), ref this.searchQuery, 128);

        ImGui.Checkbox(this.localization.Translate("browser_filter_hide_no_command"), ref this.hideEmotesWithoutCommand);

        ImGui.Spacing();
        ImGui.Separator();
        ImGui.Spacing();

        var query = this.searchQuery.Trim().ToLowerInvariant();
        var filteredEmotes = this.cachedEmotes.Where(e => {
            if (this.hideEmotesWithoutCommand && string.IsNullOrWhiteSpace(e.LocalizedCommand)) return false;
            if (string.IsNullOrEmpty(query)) return true;

            return e.Name.ToLowerInvariant().Contains(query) ||
                   (!string.IsNullOrEmpty(e.LocalizedCommand) && e.LocalizedCommand.ToLowerInvariant().Contains(query)) ||
                   (!string.IsNullOrEmpty(e.EnglishCommand) && e.EnglishCommand.ToLowerInvariant().Contains(query));
        }).ToList();

        if (ImGui.BeginTable("AllEmotesTable", 4, ImGuiTableFlags.Borders | ImGuiTableFlags.RowBg | ImGuiTableFlags.ScrollY)) {
            ImGui.TableSetupScrollFreeze(0, 1);
            ImGui.TableSetupColumn(this.localization.Translate("browser_col_icon"), ImGuiTableColumnFlags.WidthFixed, 40f);
            ImGui.TableSetupColumn(this.localization.Translate("config_common_name"), ImGuiTableColumnFlags.WidthStretch);
            ImGui.TableSetupColumn(this.localization.Translate("browser_col_command"), ImGuiTableColumnFlags.WidthStretch);
            ImGui.TableSetupColumn(this.localization.Translate("config_common_actions"), ImGuiTableColumnFlags.WidthFixed, 30f);
            ImGui.TableHeadersRow();

            foreach (var emote in filteredEmotes) {
                ImGui.TableNextRow();

                bool isSelected = this.selectionState.SelectedEmote?.Id == emote.Id;

                ImGui.TableNextColumn();
                this.DrawIconPreview(emote.IconId, 24f);

                ImGui.TableNextColumn();
                if (ImGui.Selectable($"{emote.Name}##sel_{emote.Id}", isSelected, ImGuiSelectableFlags.SpanAllColumns | ImGuiSelectableFlags.AllowItemOverlap)) {
                    this.selectionState.SelectedEmote = isSelected ? null : emote;
                }

                this.DrawContextMenu(emote);

                ImGui.TableNextColumn();
                ImGui.TextUnformatted(emote.LocalizedCommand ?? string.Empty);

                ImGui.TableNextColumn();
                ImGui.PushFont(UiBuilder.IconFont);
                if (ImGui.Button($"{FontAwesomeIcon.Play.ToIconString()}##play_{emote.Id}")) {
                    this.executionService.ExecuteEmote(emote.Id);
                }
                ImGui.PopFont();
            }
            ImGui.EndTable();
        }
    }

    private void DrawContextMenu(EmoteDisplayData emote) {
        if (ImGui.BeginPopupContextItem($"EmoteContextMenu_{emote.Id}")) {
            bool hasCommand = !string.IsNullOrWhiteSpace(emote.LocalizedCommand);

            if (hasCommand && ImGui.MenuItem(this.localization.Translate("browser_ctx_copy"))) ImGui.SetClipboardText(emote.LocalizedCommand);

            if (ImGui.MenuItem(this.localization.Translate("browser_ctx_execute"), "", false, emote.IsUnlocked)) this.executionService.ExecuteEmote(emote.Id);

            ImGui.Separator();

            if (ImGui.BeginMenu(this.localization.Translate("browser_ctx_append_macro"))) {
                if (!hasCommand) {
                    ImGui.MenuItem(this.localization.Translate("browser_details_no_command"), "", false, false);
                }
                else {
                    var unlockedMacros = this.macroService.GetMacros().Where(m => !m.IsLocked).ToList();
                    if (!unlockedMacros.Any()) {
                        ImGui.MenuItem(this.localization.Translate("browser_ctx_no_unlocked_macros"), "", false, false);
                    }
                    else {
                        foreach (var m in unlockedMacros) {
                            if (ImGui.MenuItem(m.Name)) this.macroService.AppendToMacro(m.Id, emote.LocalizedCommand);
                        }
                    }
                }
                ImGui.EndMenu();
            }
            ImGui.EndPopup();
        }
    }

    private void DrawIconPreview(uint iconId, float size) {
        try {
            var lookup = new GameIconLookup { IconId = iconId, HiRes = false };
            var iconWrap = this.textureProvider.GetFromGameIcon(lookup).GetWrapOrDefault();

            if (iconWrap != null) ImGui.Image(iconWrap.Handle, new Vector2(size, size));
            else ImGui.Dummy(new Vector2(size, size));
        }
        catch (Exception) {
            ImGui.Dummy(new Vector2(size, size));
        }
    }

    public void DrawSidePanel() { }
    public void Dispose() { }
}