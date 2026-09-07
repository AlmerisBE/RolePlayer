namespace RolePlayer.UI.MainWindow.Components.Macros;

using Dalamud.Bindings.ImGui;
using Dalamud.Interface;
using Dalamud.Interface.Textures;
using Dalamud.Interface.Textures.Internal;
using Dalamud.Plugin.Services;
using RolePlayer.UI.EmoteBrowser.Contracts;
using RolePlayer.UI.Hotbar.Contracts;
using RolePlayer.UI.Localization.Contracts;
using RolePlayer.UI.MainWindow.Contracts;
using System;
using System.Linq;
using System.Numerics;

public class MacroListComponent {
    private IMacroManagementService macroService;
    private IMacroExecutionService macroExecutionService;
    private ILocalizationService localization;
    private ITextureProvider textureProvider;
    private IGroupManagementService groupService;
    private ITagManagementService tagService;
    private IMacroSelectionState selectionState;

    private MacroSearchComponent searchComponent;
    private MacroContextMenuComponent contextMenuComponent;
    private MacroDeleteModalComponent deleteModalComponent;

    public MacroListComponent(
        IMacroManagementService macroService,
        IMacroExecutionService macroExecutionService,
        ILocalizationService localization,
        ITextureProvider textureProvider,
        IGroupManagementService groupService,
        ITagManagementService tagService,
        IMacroSelectionState selectionState,
        MacroSearchComponent searchComponent,
        MacroContextMenuComponent contextMenuComponent,
        MacroDeleteModalComponent deleteModalComponent) {

        this.macroService = macroService;
        this.macroExecutionService = macroExecutionService;
        this.localization = localization;
        this.textureProvider = textureProvider;
        this.groupService = groupService;
        this.tagService = tagService;
        this.selectionState = selectionState;
        this.searchComponent = searchComponent;
        this.contextMenuComponent = contextMenuComponent;
        this.deleteModalComponent = deleteModalComponent;
    }

    public void Draw() {
        var macros = this.macroService.GetMacros().ToList();
        if (macros.Count == 0) return;

        var query = this.searchComponent.Query.Trim().ToLowerInvariant();
        var filteredMacros = macros.Where(m => {
            var tags = this.tagService.GetTagsForMacro(m.Id);
            return string.IsNullOrEmpty(query) ||
                   m.Name.ToLowerInvariant().Contains(query) ||
                   tags.Any(t => t.ToLowerInvariant().Contains(query));
        }).ToList();

        var groupedMacros = filteredMacros
            .GroupBy(m => {
                var group = this.groupService.GetGroupForMacro(m.Id);
                return string.IsNullOrEmpty(group) ? this.localization.Translate("browser_ungrouped") : group;
            })
            .OrderBy(g => g.Key);

        foreach (var group in groupedMacros) {
            if (ImGui.CollapsingHeader($"{group.Key} ({group.Count()})###MacroGroup_{group.Key}", ImGuiTreeNodeFlags.DefaultOpen)) {
                if (ImGui.BeginTable($"MacrosTable_{group.Key}", 3, ImGuiTableFlags.Borders | ImGuiTableFlags.RowBg | ImGuiTableFlags.SizingFixedFit)) {
                    ImGui.TableSetupColumn(this.localization.Translate("config_macro_col_icon"), ImGuiTableColumnFlags.WidthFixed, 40f);
                    ImGui.TableSetupColumn(this.localization.Translate("config_common_name"), ImGuiTableColumnFlags.WidthStretch);
                    ImGui.TableSetupColumn(this.localization.Translate("config_common_actions"), ImGuiTableColumnFlags.WidthFixed, 68f);

                    ImGui.TableHeadersRow();

                    foreach (var macro in group) {
                        float rowHeight = 32f;
                        ImGui.TableNextRow(ImGuiTableRowFlags.None, rowHeight);

                        bool isSelected = this.selectionState.SelectedMacro?.Id == macro.Id;

                        ImGui.TableNextColumn();
                        float iconSize = 24f;
                        float startY = ImGui.GetCursorPosY() - ImGui.GetStyle().CellPadding.Y;
                        ImGui.SetCursorPosY(startY + (rowHeight - iconSize) / 2f);
                        this.DrawIconPreview(macro.IconId, iconSize);

                        ImGui.TableNextColumn();
                        ImGui.PushStyleVar(ImGuiStyleVar.SelectableTextAlign, new Vector2(0.0f, 0.5f));
                        float selectableHeight = rowHeight - (ImGui.GetStyle().CellPadding.Y * 2);

                        if (ImGui.Selectable($"{macro.Name}##sel_{macro.Id}", isSelected, ImGuiSelectableFlags.SpanAllColumns | ImGuiSelectableFlags.AllowItemOverlap, new Vector2(0, selectableHeight))) {
                            this.selectionState.SelectedMacro = isSelected ? null : macro;
                        }
                        ImGui.PopStyleVar();

                        this.contextMenuComponent.Draw(macro);

                        ImGui.TableNextColumn();
                        float buttonHeight = ImGui.GetFrameHeight();
                        startY = ImGui.GetCursorPosY() - ImGui.GetStyle().CellPadding.Y;
                        ImGui.SetCursorPosY(startY + (rowHeight - buttonHeight) / 2f);

                        ImGui.PushFont(UiBuilder.IconFont);

                        ImGui.PushStyleColor(ImGuiCol.Button, new Vector4(0.2f, 0.6f, 0.2f, 1.0f));
                        if (ImGui.Button($"{FontAwesomeIcon.Play.ToIconString()}##Play_{macro.Id}")) this.macroExecutionService.Execute(macro);
                        ImGui.PopStyleColor();

                        if (ImGui.IsItemHovered()) {
                            ImGui.PopFont();
                            ImGui.SetTooltip(this.localization.Translate("config_macro_execute"));
                            ImGui.PushFont(UiBuilder.IconFont);
                        }

                        ImGui.SameLine();

                        ImGui.PushStyleColor(ImGuiCol.Button, new Vector4(0.8f, 0.2f, 0.2f, 1.0f));
                        if (ImGui.Button($"{FontAwesomeIcon.Trash.ToIconString()}##Del_{macro.Id}")) {
                            this.deleteModalComponent.Open(macro.Id);
                        }
                        ImGui.PopStyleColor();
                        ImGui.PopFont();
                    }
                    ImGui.EndTable();
                }
            }
        }

        this.deleteModalComponent.Draw();
    }

    private void DrawIconPreview(uint iconId, float size) {
        try {
            var lookup = new GameIconLookup { IconId = iconId, HiRes = false };
            var iconWrap = this.textureProvider.GetFromGameIcon(lookup).GetWrapOrDefault();

            if (iconWrap != null) ImGui.Image(iconWrap.Handle, new Vector2(size, size));
            else ImGui.Dummy(new Vector2(size, size));
        }
        catch (IconNotFoundException) {
            ImGui.Dummy(new Vector2(size, size));
        }
    }
}