namespace RolePlayer.UI.MainWindow.Tabs;

using Dalamud.Bindings.ImGui;
using Dalamud.Interface;
using Dalamud.Interface.Textures;
using Dalamud.Interface.Textures.Internal;
using Dalamud.Plugin.Services;
using RolePlayer.Core.Macros.Models;
using RolePlayer.UI.EmoteBrowser.Contracts;
using RolePlayer.UI.Hotbar.Components;
using RolePlayer.UI.Hotbar.Contracts;
using RolePlayer.UI.Localization.Contracts;
using RolePlayer.UI.MainWindow.Contracts;
using RolePlayer.UI.MainWindow.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

public class MacrosTab : IEmoteBrowserTab, IDisposable {
    private IMacroManagementService macroService;
    private IMacroExecutionService macroExecutionService;
    private ILocalizationService localization;
    private ITextureProvider textureProvider;
    private HotbarManagerComponent hotbarManager;
    private IAutoTranslateService autoTranslateService;

    private string newMacroName = string.Empty;
    private RoleplayMacro? selectedMacro;
    private Guid macroToDelete = Guid.Empty;
    private bool isDeleteDialogOpen = false;

    private string autoTranslateSearch = string.Empty;
    private List<AutoTranslateResult> autoTranslateResults = new();

    public string TabName => this.localization.Translate("main_tab_macros");
    public int SortOrder => 20;
    public bool IsSidePanelOpen => this.selectedMacro != null;

    public MacrosTab(
        IMacroManagementService macroService,
        IMacroExecutionService macroExecutionService,
        ILocalizationService localization,
        ITextureProvider textureProvider,
        HotbarManagerComponent hotbarManager,
        IAutoTranslateService autoTranslateService) {

        this.macroService = macroService;
        this.macroExecutionService = macroExecutionService;
        this.localization = localization;
        this.textureProvider = textureProvider;
        this.hotbarManager = hotbarManager;
        this.autoTranslateService = autoTranslateService;
    }

    public void Draw() {
        ImGui.Text(this.localization.Translate("config_macro_create"));

        float availableWidth = ImGui.GetContentRegionAvail().X;
        float buttonWidth = 32f;
        float spacing = ImGui.GetStyle().ItemSpacing.X;

        ImGui.SetNextItemWidth(availableWidth - buttonWidth - spacing);
        ImGui.InputTextWithHint("##NewMacroName", this.localization.Translate("config_macro_name_hint"), ref this.newMacroName, 64);
        ImGui.SameLine();

        ImGui.PushFont(UiBuilder.IconFont);
        if (ImGui.Button($"{FontAwesomeIcon.Plus.ToIconString()}##AddMacro", new Vector2(buttonWidth, 0)) && !string.IsNullOrWhiteSpace(this.newMacroName)) {
            var newMacro = new RoleplayMacro {
                Name = this.newMacroName.Trim(),
                IconId = 66001
            };
            this.macroService.CreateMacro(newMacro);
            this.selectedMacro = newMacro;
            this.newMacroName = string.Empty;
        }
        ImGui.PopFont();

        ImGui.Spacing();
        ImGui.Separator();
        ImGui.Spacing();

        var macros = this.macroService.GetMacros().ToList();
        if (macros.Count == 0) return;

        if (ImGui.BeginTable("MacrosTable", 3, ImGuiTableFlags.Borders | ImGuiTableFlags.RowBg | ImGuiTableFlags.SizingFixedFit)) {
            ImGui.TableSetupColumn(this.localization.Translate("config_common_name"), ImGuiTableColumnFlags.WidthStretch);
            ImGui.TableSetupColumn(this.localization.Translate("config_macro_col_icon"), ImGuiTableColumnFlags.WidthFixed, 40f);
            ImGui.TableSetupColumn(this.localization.Translate("config_common_actions"), ImGuiTableColumnFlags.WidthFixed, 68f);

            ImGui.TableHeadersRow();

            foreach (var macro in macros) {
                float rowHeight = 32f;
                ImGui.TableNextRow(ImGuiTableRowFlags.None, rowHeight);

                bool isSelected = this.selectedMacro?.Id == macro.Id;

                ImGui.TableNextColumn();
                ImGui.PushStyleVar(ImGuiStyleVar.SelectableTextAlign, new Vector2(0.0f, 0.5f));
                float selectableHeight = rowHeight - (ImGui.GetStyle().CellPadding.Y * 2);

                if (ImGui.Selectable($"{macro.Name}##sel_{macro.Id}", isSelected, ImGuiSelectableFlags.SpanAllColumns | ImGuiSelectableFlags.AllowItemOverlap, new Vector2(0, selectableHeight))) {
                    this.selectedMacro = isSelected ? null : macro;
                    this.autoTranslateSearch = string.Empty;
                    this.autoTranslateResults.Clear();
                }
                ImGui.PopStyleVar();

                ImGui.TableNextColumn();
                float iconSize = 24f;
                float startY = ImGui.GetCursorPosY() - ImGui.GetStyle().CellPadding.Y;
                ImGui.SetCursorPosY(startY + (rowHeight - iconSize) / 2f);
                this.DrawIconPreview(macro.IconId, iconSize);

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
                    this.macroToDelete = macro.Id;
                    this.isDeleteDialogOpen = true;
                }
                ImGui.PopStyleColor();
                ImGui.PopFont();
            }
            ImGui.EndTable();
        }

        this.DrawDeleteConfirmationModal();
    }

    public void DrawSidePanel() {
        if (this.selectedMacro == null) return;

        bool changed = false;

        string playIcon = FontAwesomeIcon.Play.ToIconString();
        string closeIcon = FontAwesomeIcon.Times.ToIconString();

        ImGui.PushFont(UiBuilder.IconFont);
        var playBtnWidth = ImGui.CalcTextSize(playIcon).X + ImGui.GetStyle().FramePadding.X * 2;
        var closeBtnWidth = ImGui.CalcTextSize(closeIcon).X + ImGui.GetStyle().FramePadding.X * 2;
        ImGui.PopFont();

        if (ImGui.BeginTable("MacroSettingsHeaderTable", 3)) {
            ImGui.TableSetupColumn("Title", ImGuiTableColumnFlags.WidthStretch);
            ImGui.TableSetupColumn("PlayBtn", ImGuiTableColumnFlags.WidthFixed, playBtnWidth);
            ImGui.TableSetupColumn("CloseBtn", ImGuiTableColumnFlags.WidthFixed, closeBtnWidth);

            ImGui.TableNextRow();
            ImGui.TableNextColumn();
            ImGui.AlignTextToFramePadding();
            ImGui.SetWindowFontScale(1.3f);

            string title = string.IsNullOrWhiteSpace(this.selectedMacro.Name) ? this.localization.Translate("config_macro_settings") : this.selectedMacro.Name;
            ImGui.TextUnformatted(title);
            ImGui.SetWindowFontScale(1.0f);

            ImGui.TableNextColumn();
            ImGui.PushFont(UiBuilder.IconFont);
            ImGui.PushStyleColor(ImGuiCol.Button, new Vector4(0.2f, 0.6f, 0.2f, 1.0f));
            if (ImGui.Button($"{playIcon}##PlayMacroDetails")) this.macroExecutionService.Execute(this.selectedMacro);
            ImGui.PopStyleColor();
            ImGui.PopFont();

            if (ImGui.IsItemHovered()) ImGui.SetTooltip(this.localization.Translate("config_macro_execute"));

            ImGui.TableNextColumn();
            ImGui.PushFont(UiBuilder.IconFont);
            if (ImGui.Button($"{closeIcon}##CloseMacroDetails")) {
                this.selectedMacro = null;
                ImGui.PopFont();
                ImGui.EndTable();
                return;
            }
            ImGui.PopFont();

            ImGui.EndTable();
        }

        ImGui.Separator();
        ImGui.Spacing();

        // Calcul exact de la hauteur nécessaire pour occuper l'espace du label ET du champ texte
        float totalHeight = ImGui.GetTextLineHeight() + ImGui.GetStyle().ItemSpacing.Y + ImGui.GetFrameHeight();
        float btnSizeXY = totalHeight;
        float imgSize = btnSizeXY - (ImGui.GetStyle().FramePadding.Y * 2);
        float inputWidth = ImGui.GetContentRegionAvail().X - btnSizeXY - ImGui.GetStyle().ItemSpacing.X;

        // Le groupe permet d'encapsuler la hauteur totale des deux éléments de gauche
        ImGui.BeginGroup();
        ImGui.TextDisabled(this.localization.Translate("config_common_name"));

        string name = this.selectedMacro.Name;
        ImGui.SetNextItemWidth(inputWidth);
        if (ImGui.InputText("##MacroNameEdit", ref name, 64)) {
            this.selectedMacro.Name = name;
            changed = true;
        }
        ImGui.EndGroup();

        // SameLine aligne la suite sur le haut du groupe précédent
        ImGui.SameLine();

        bool openIconPicker = false;
        ImGui.PushID("IconSelectButton");
        try {
            var lookup = new GameIconLookup { IconId = this.selectedMacro.IconId, HiRes = false };
            var iconWrap = this.textureProvider.GetFromGameIcon(lookup).GetWrapOrDefault();

            if (iconWrap != null && ImGui.ImageButton(iconWrap.Handle, new Vector2(imgSize, imgSize))) openIconPicker = true;
            if (iconWrap == null && ImGui.Button("?", new Vector2(btnSizeXY, btnSizeXY))) openIconPicker = true;
        }
        catch (IconNotFoundException) {
            if (ImGui.Button("?", new Vector2(btnSizeXY, btnSizeXY))) openIconPicker = true;
        }
        ImGui.PopID();

        if (ImGui.IsItemHovered()) ImGui.SetTooltip(this.localization.Translate("config_macro_icon_select"));

        if (openIconPicker) ImGui.OpenPopup("IconPickerPopup");

        this.DrawIconPickerPopup(ref changed);

        ImGui.Spacing();
        ImGui.Separator();
        ImGui.Spacing();

        ImGui.TextDisabled(this.localization.Translate("config_macro_col_content"));
        ImGui.Spacing();

        string content = this.selectedMacro.Content;
        float inputHeight = ImGui.GetTextLineHeight() * 10f;

        if (ImGui.InputTextMultiline("##MacroContent", ref content, 8192, new Vector2(-1, inputHeight))) {
            this.selectedMacro.Content = content;
            changed = true;
        }

        if (ImGui.IsItemHovered()) ImGui.SetTooltip(this.localization.Translate("config_macro_lines_hint"));

        ImGui.Spacing();
        ImGui.Separator();
        ImGui.Spacing();

        ImGui.TextDisabled(this.localization.Translate("config_macro_autotranslate_title"));
        ImGui.Spacing();

        ImGui.TextWrapped(this.localization.Translate("config_macro_autotranslate_desc"));
        ImGui.Spacing();

        ImGui.SetNextItemWidth(-1f);

        if (ImGui.InputTextWithHint("##AutoTranslateSearch", this.localization.Translate("config_macro_autotranslate_hint"), ref this.autoTranslateSearch, 64)) {
            if (this.autoTranslateSearch.Length >= 2) this.autoTranslateResults = this.autoTranslateService.Search(this.autoTranslateSearch).ToList();
            else this.autoTranslateResults.Clear();
        }

        if (this.autoTranslateResults.Count > 0) {
            if (ImGui.BeginChild("AutoTranslateResultsList", new Vector2(0, 150), true)) {
                foreach (var result in this.autoTranslateResults) {
                    if (ImGui.Selectable($"{result.DisplayText}##{result.Payload}")) {
                        this.selectedMacro.Content += result.Payload;
                        changed = true;
                        this.autoTranslateSearch = string.Empty;
                        this.autoTranslateResults.Clear();
                        break;
                    }
                }
                ImGui.EndChild();
            }
        }

        if (changed) {
            this.macroService.UpdateMacro(this.selectedMacro.Id, this.selectedMacro.Name, this.selectedMacro.Content, this.selectedMacro.IconId);
        }
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

    private void DrawIconPickerPopup(ref bool changed) {
        ImGui.SetNextWindowSize(new Vector2(340, 400), ImGuiCond.Appearing);

        if (ImGui.BeginPopup("IconPickerPopup")) {
            ImGui.TextDisabled(this.localization.Translate("config_macro_icon_picker"));
            ImGui.Separator();

            if (ImGui.BeginTabBar("IconPickerTabs")) {
                if (ImGui.BeginTabItem(this.localization.Translate("config_macro_icon_tab_macros"))) {
                    this.DrawIconGrid("MacroIconsGrid", 66001, 66344, null, ref changed);
                    ImGui.EndTabItem();
                }

                if (ImGui.BeginTabItem(this.localization.Translate("config_macro_icon_tab_emotes"))) {
                    var emoteIcons = this.hotbarManager.GetEmoteCache().Select(e => e.IconId).Distinct().ToList();
                    this.DrawIconGrid("EmoteIconsGrid", 0, 0, emoteIcons, ref changed);
                    ImGui.EndTabItem();
                }

                ImGui.EndTabBar();
            }
            ImGui.EndPopup();
        }
    }

    private void DrawIconGrid(string id, uint startId, uint endId, List<uint>? specificIcons, ref bool changed) {
        if (ImGui.BeginChild(id, new Vector2(0, 300), false, ImGuiWindowFlags.AlwaysVerticalScrollbar)) {
            int columns = Math.Max(1, (int)(ImGui.GetContentRegionAvail().X / 48f));

            if (ImGui.BeginTable($"{id}Table", columns, ImGuiTableFlags.SizingFixedFit)) {
                var iconsToRender = specificIcons ?? Enumerable.Range((int)startId, (int)(endId - startId + 1)).Select(i => (uint)i).ToList();
                int drawnIconsCount = 0;

                foreach (uint iconId in iconsToRender) {
                    try {
                        var lookup = new GameIconLookup { IconId = iconId, HiRes = false };
                        var iconWrap = this.textureProvider.GetFromGameIcon(lookup).GetWrapOrDefault();

                        if (iconWrap != null) {
                            if (drawnIconsCount % columns == 0) ImGui.TableNextRow();

                            ImGui.TableNextColumn();

                            ImGui.PushID($"icon_{iconId}");
                            if (ImGui.ImageButton(iconWrap.Handle, new Vector2(38, 38))) {
                                this.selectedMacro!.IconId = iconId;
                                changed = true;
                                ImGui.CloseCurrentPopup();
                            }
                            ImGui.PopID();

                            drawnIconsCount++;
                        }
                    }
                    catch (IconNotFoundException) { }
                }
                ImGui.EndTable();
            }
            ImGui.EndChild();
        }
    }

    private void DrawDeleteConfirmationModal() {
        if (this.isDeleteDialogOpen) {
            ImGui.OpenPopup(this.localization.Translate("config_macro_del_title"));
            this.isDeleteDialogOpen = false;
        }

        if (ImGui.BeginPopupModal(this.localization.Translate("config_macro_del_title"), ImGuiWindowFlags.AlwaysAutoResize | ImGuiWindowFlags.NoSavedSettings)) {
            var macroName = this.macroService.GetMacros().FirstOrDefault(m => m.Id == this.macroToDelete)?.Name ?? "Unknown";
            ImGui.Text(this.localization.Translate("config_macro_del_desc", macroName));
            ImGui.TextColored(new Vector4(0.8f, 0.2f, 0.2f, 1.0f), this.localization.Translate("config_macro_del_warn"));

            ImGui.Spacing();
            ImGui.Separator();
            ImGui.Spacing();

            if (ImGui.Button(this.localization.Translate("config_common_yes_delete"), new Vector2(120, 0))) {
                this.macroService.DeleteMacro(this.macroToDelete);
                if (this.selectedMacro?.Id == this.macroToDelete) this.selectedMacro = null;
                ImGui.CloseCurrentPopup();
            }
            ImGui.SameLine();
            if (ImGui.Button(this.localization.Translate("config_common_cancel"), new Vector2(120, 0))) ImGui.CloseCurrentPopup();

            ImGui.EndPopup();
        }
    }

    public void Dispose() { }
}