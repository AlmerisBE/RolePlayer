namespace RolePlayer.UI.EmoteBrowser.Components;

using Dalamud.Bindings.ImGui;
using Dalamud.Game;
using Dalamud.Interface;
using Dalamud.Interface.Textures;
using Dalamud.Plugin.Services;
using RolePlayer.Core.Configuration.Models;
using RolePlayer.Core.Emotes.Models;
using RolePlayer.UI.EmoteBrowser.Contracts;
using RolePlayer.UI.EmoteBrowser.Models;
using RolePlayer.UI.Localization.Contracts;
using System;
using System.Linq;
using System.Numerics;

public class EmoteListComponent {
    private IEmoteSelectionState selectionState;
    private IEmoteExecutionService executionService;
    private ILocalizationService localization;
    private ITextureProvider textureProvider;
    private IClientState clientState;
    private EmoteContextMenuComponent contextMenuComponent;

    public EmoteListComponent(
        IEmoteSelectionState selectionState,
        IEmoteExecutionService executionService,
        ILocalizationService localization,
        ITextureProvider textureProvider,
        IClientState clientState,
        EmoteContextMenuComponent contextMenuComponent) {

        this.selectionState = selectionState;
        this.executionService = executionService;
        this.localization = localization;
        this.textureProvider = textureProvider;
        this.clientState = clientState;
        this.contextMenuComponent = contextMenuComponent;
    }

    public void Draw(IEmoteBrowserPresenter presenter, EmoteContext context) {
        bool refreshRequested = false;

        if (ImGui.BeginChild("EmoteListScrollArea", new Vector2(0, 0), false, ImGuiWindowFlags.None)) {
            var tableFlags = ImGuiTableFlags.Borders | ImGuiTableFlags.RowBg | ImGuiTableFlags.Sortable | ImGuiTableFlags.SizingFixedFit;

            foreach (var groupKvp in presenter.GroupedEmotes.OrderBy(k => k.Key)) {
                bool isNodeOpen = true;

                if (context.CurrentGrouping != GroupingMode.None) isNodeOpen = ImGui.CollapsingHeader($"{groupKvp.Key} ({groupKvp.Value.Count})###Header_{groupKvp.Key}", ImGuiTreeNodeFlags.DefaultOpen);

                if (isNodeOpen) {
                    if (ImGui.BeginTable($"AllEmotesTable_{groupKvp.Key}", 4, tableFlags)) {
                        ImGui.TableSetupColumn(this.localization.Translate("browser_col_icon"), ImGuiTableColumnFlags.WidthFixed | ImGuiTableColumnFlags.NoSort, 32f);
                        ImGui.TableSetupColumn(this.localization.Translate("browser_col_name"), ImGuiTableColumnFlags.WidthStretch, 0.4f);
                        ImGui.TableSetupColumn(this.localization.Translate("browser_col_command"), ImGuiTableColumnFlags.WidthStretch, 0.4f);
                        ImGui.TableSetupColumn(this.localization.Translate("browser_col_action"), ImGuiTableColumnFlags.WidthFixed | ImGuiTableColumnFlags.NoSort, 40f);
                        ImGui.TableHeadersRow();

                        var sortSpecs = ImGui.TableGetSortSpecs();
                        if (sortSpecs.SpecsDirty) {
                            presenter.SetSort(sortSpecs.Specs.ColumnIndex, sortSpecs.Specs.SortDirection == ImGuiSortDirection.Descending);
                            sortSpecs.SpecsDirty = false;
                        }

                        foreach (var emote in groupKvp.Value) {
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

                            ImGui.TableNextColumn();
                            if (ImGui.Selectable($"##select_{emote.Id}", isSelected, ImGuiSelectableFlags.SpanAllColumns | ImGuiSelectableFlags.AllowItemOverlap, new Vector2(0, 24))) this.selectionState.SelectedEmote = isSelected ? null : emote;

                            // Captures the need to refresh without mutating the collection during iteration
                            if (this.contextMenuComponent.Draw(emote, context)) refreshRequested = true;

                            ImGui.SameLine();

                            this.DrawIconPreview(emote, 24f);

                            ImGui.TableNextColumn();
                            var displayName = emote.IsModded ? $"★ {emote.Name}" : emote.Name;
                            ImGui.AlignTextToFramePadding();
                            ImGui.Text(displayName);

                            ImGui.TableNextColumn();
                            ImGui.AlignTextToFramePadding();
                            var commandText = emote.LocalizedCommand;

                            if (this.clientState.ClientLanguage != ClientLanguage.English && !string.IsNullOrEmpty(emote.EnglishCommand) && emote.EnglishCommand != emote.LocalizedCommand) commandText += $" / {emote.EnglishCommand}";

                            ImGui.Text(commandText);

                            ImGui.TableNextColumn();
                            if (emote.IsUnlocked) {
                                ImGui.PushFont(UiBuilder.IconFont);
                                if (ImGui.Button($"{FontAwesomeIcon.Play.ToIconString()}##{emote.Id}", new Vector2(-1, 24))) this.executionService.ExecuteEmote(emote.Id);
                                ImGui.PopFont();

                                if (ImGui.IsItemHovered()) ImGui.SetTooltip(this.localization.Translate("browser_ctx_execute"));
                            }

                            if (hasCustomColor) ImGui.PopStyleColor();
                        }
                        ImGui.EndTable();
                    }
                }
            }
        }
        ImGui.EndChild();

        // Safely apply filters after the ImGui table rendering is complete to avoid InvalidOperationException
        if (refreshRequested) presenter.ApplyFilters();
    }

    private void DrawIconPreview(EnrichedEmote emote, float size) {
        if (emote.IconId == 0) {
            ImGui.Dummy(new Vector2(size, size));
            return;
        }

        try {
            var lookup = new GameIconLookup { IconId = emote.IconId, HiRes = false };
            var iconWrap = this.textureProvider.GetFromGameIcon(lookup).GetWrapOrDefault();

            if (iconWrap != null) {
                var cursorPos = ImGui.GetCursorScreenPos();
                ImGui.Image(iconWrap.Handle, new Vector2(size, size));

                if (emote.HasVariations) {
                    var drawList = ImGui.GetWindowDrawList();
                    ImGui.PushFont(UiBuilder.IconFont);
                    var indicatorText = FontAwesomeIcon.Sync.ToIconString();
                    var textSize = ImGui.CalcTextSize(indicatorText);
                    ImGui.PopFont();

                    var indicatorPos = new Vector2(cursorPos.X + size - textSize.X - 1f, cursorPos.Y + size - textSize.Y - 1f);
                    drawList.AddText(UiBuilder.IconFont, ImGui.GetFontSize(), new Vector2(indicatorPos.X + 1, indicatorPos.Y + 1), 0xFF000000, indicatorText);
                    drawList.AddText(UiBuilder.IconFont, ImGui.GetFontSize(), indicatorPos, 0xFF40DD40, indicatorText);
                }
            }
            else {
                ImGui.Dummy(new Vector2(size, size));
            }
        }
        catch (Exception) {
            ImGui.Dummy(new Vector2(size, size));
        }
    }
}