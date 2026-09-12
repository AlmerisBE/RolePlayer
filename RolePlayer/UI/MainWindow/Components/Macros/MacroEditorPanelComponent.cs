namespace RolePlayer.UI.MainWindow.Components.Macros;

using Dalamud.Bindings.ImGui;
using Dalamud.Interface;
using Dalamud.Interface.Textures;
using Dalamud.Interface.Textures.Internal;
using Dalamud.Plugin.Services;
using RolePlayer.Core.Configuration.Contracts;
using RolePlayer.Core.Emotes.Contracts;
using RolePlayer.Core.Macros.Models;
using RolePlayer.UI.EmoteBrowser.Contracts;
using RolePlayer.UI.Hotbar.Contracts;
using RolePlayer.UI.Hotbar.Models;
using RolePlayer.UI.Localization.Contracts;
using RolePlayer.UI.MainWindow.Contracts;
using RolePlayer.UI.MainWindow.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

public class MacroEditorPanelComponent {
    private IMacroManagementService macroService;
    private IMacroExecutionService macroExecutionService;
    private ILocalizationService localization;
    private ITextureProvider textureProvider;
    private IEmoteCache emoteCache;
    private IAutoTranslateService autoTranslateService;
    private IGroupManagementService groupService;
    private ITagManagementService tagService;
    private IConfigurationService configurationService;
    private IContextManagementService contextService;
    private IMacroSelectionState selectionState;

    private string autoTranslateSearch = string.Empty;
    private List<AutoTranslateResult> autoTranslateResults = new();

    public MacroEditorPanelComponent(
        IMacroManagementService macroService,
        IMacroExecutionService macroExecutionService,
        ILocalizationService localization,
        ITextureProvider textureProvider,
        IEmoteCache emoteCache,
        IAutoTranslateService autoTranslateService,
        IGroupManagementService groupService,
        ITagManagementService tagService,
        IConfigurationService configurationService,
        IContextManagementService contextService,
        IMacroSelectionState selectionState) {

        this.macroService = macroService;
        this.macroExecutionService = macroExecutionService;
        this.localization = localization;
        this.textureProvider = textureProvider;
        this.emoteCache = emoteCache;
        this.autoTranslateService = autoTranslateService;
        this.groupService = groupService;
        this.tagService = tagService;
        this.configurationService = configurationService;
        this.contextService = contextService;
        this.selectionState = selectionState;
    }

    public void Draw() {
        var macro = this.selectionState.SelectedMacro;
        if (macro == null) return;

        bool changed = false;

        string copyIcon = FontAwesomeIcon.Copy.ToIconString();
        string lockIcon = macro.IsLocked ? FontAwesomeIcon.Lock.ToIconString() : FontAwesomeIcon.Unlock.ToIconString();
        string playIcon = FontAwesomeIcon.Play.ToIconString();
        string closeIcon = FontAwesomeIcon.Times.ToIconString();

        ImGui.PushFont(UiBuilder.IconFont);
        var copyBtnWidth = ImGui.CalcTextSize(copyIcon).X + ImGui.GetStyle().FramePadding.X * 2;
        var lockBtnWidth = ImGui.CalcTextSize(lockIcon).X + ImGui.GetStyle().FramePadding.X * 2;
        var playBtnWidth = ImGui.CalcTextSize(playIcon).X + ImGui.GetStyle().FramePadding.X * 2;
        var closeBtnWidth = ImGui.CalcTextSize(closeIcon).X + ImGui.GetStyle().FramePadding.X * 2;
        ImGui.PopFont();

        if (ImGui.BeginTable("MacroSettingsHeaderTable", 5)) {
            ImGui.TableSetupColumn("Title", ImGuiTableColumnFlags.WidthStretch);
            ImGui.TableSetupColumn("CopyBtn", ImGuiTableColumnFlags.WidthFixed, copyBtnWidth);
            ImGui.TableSetupColumn("LockBtn", ImGuiTableColumnFlags.WidthFixed, lockBtnWidth);
            ImGui.TableSetupColumn("PlayBtn", ImGuiTableColumnFlags.WidthFixed, playBtnWidth);
            ImGui.TableSetupColumn("CloseBtn", ImGuiTableColumnFlags.WidthFixed, closeBtnWidth);

            ImGui.TableNextRow();
            ImGui.TableNextColumn();
            ImGui.AlignTextToFramePadding();
            ImGui.SetWindowFontScale(1.3f);

            string title = string.IsNullOrWhiteSpace(macro.Name) ? this.localization.Translate("config_macro_settings") : macro.Name;
            ImGui.TextUnformatted(title);
            ImGui.SetWindowFontScale(1.0f);

            ImGui.TableNextColumn();
            ImGui.PushFont(UiBuilder.IconFont);
            if (ImGui.Button($"{copyIcon}##CopyMacroDetails")) ImGui.SetClipboardText(macro.Content);
            ImGui.PopFont();
            if (ImGui.IsItemHovered()) ImGui.SetTooltip(this.localization.Translate("config_macro_copy"));

            ImGui.TableNextColumn();
            ImGui.PushFont(UiBuilder.IconFont);

            bool wasLocked = macro.IsLocked;
            if (wasLocked) ImGui.PushStyleColor(ImGuiCol.Text, new Vector4(0.8f, 0.2f, 0.2f, 1.0f));

            if (ImGui.Button($"{lockIcon}##LockMacroDetails")) {
                macro.IsLocked = !macro.IsLocked;
                changed = true;
            }

            if (wasLocked) ImGui.PopStyleColor();
            ImGui.PopFont();
            if (ImGui.IsItemHovered()) ImGui.SetTooltip(this.localization.Translate("config_macro_toggle_lock"));

            ImGui.TableNextColumn();
            ImGui.PushFont(UiBuilder.IconFont);
            ImGui.PushStyleColor(ImGuiCol.Button, new Vector4(0.2f, 0.6f, 0.2f, 1.0f));
            if (ImGui.Button($"{playIcon}##PlayMacroDetails")) this.macroExecutionService.Execute(macro);
            ImGui.PopStyleColor();
            ImGui.PopFont();
            if (ImGui.IsItemHovered()) ImGui.SetTooltip(this.localization.Translate("config_macro_execute"));

            ImGui.TableNextColumn();
            ImGui.PushFont(UiBuilder.IconFont);
            if (ImGui.Button($"{closeIcon}##CloseMacroDetails")) {
                this.selectionState.SelectedMacro = null;
                ImGui.PopFont();
                ImGui.EndTable();
                return;
            }
            ImGui.PopFont();

            ImGui.EndTable();
        }

        ImGui.Separator();

        // Zone de défilement isolée pour le contenu
        if (ImGui.BeginChild("MacroEditorScrollArea")) {
            ImGui.Spacing();

            float totalHeight = ImGui.GetTextLineHeight() + ImGui.GetStyle().ItemSpacing.Y + ImGui.GetFrameHeight();
            float btnSizeXY = totalHeight;
            float imgSize = btnSizeXY - (ImGui.GetStyle().FramePadding.Y * 2);
            float inputWidth = ImGui.GetContentRegionAvail().X - btnSizeXY - ImGui.GetStyle().ItemSpacing.X;

            ImGui.BeginDisabled(macro.IsLocked);

            ImGui.BeginGroup();
            ImGui.TextDisabled(this.localization.Translate("config_common_name"));

            string name = macro.Name;
            ImGui.SetNextItemWidth(inputWidth);
            if (ImGui.InputText("##MacroNameEdit", ref name, 64)) {
                macro.Name = name;
                changed = true;
            }
            ImGui.EndGroup();

            ImGui.SameLine();

            bool openIconPicker = false;
            ImGui.PushID("IconSelectButton");
            try {
                var lookup = new GameIconLookup { IconId = macro.IconId, HiRes = false };
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

            this.DrawIconPickerPopup(macro, ref changed);

            ImGui.EndDisabled();

            ImGui.Spacing();
            ImGui.Separator();
            ImGui.Spacing();

            this.DrawStaticHotbarAssignment(macro.Id);

            ImGui.Spacing();
            ImGui.Separator();
            ImGui.Spacing();

            ImGui.TextDisabled(this.localization.Translate("config_macro_group"));
            ImGui.SetNextItemWidth(-1f);
            var groups = this.groupService.GetGroups().Select(g => g.Name).ToList();
            var currentGroup = this.groupService.GetGroupForMacro(macro.Id);

            if (ImGui.BeginCombo("##MacroGroupCombo", string.IsNullOrEmpty(currentGroup) ? this.localization.Translate("browser_none") : currentGroup)) {
                if (ImGui.Selectable(this.localization.Translate("browser_none"), string.IsNullOrEmpty(currentGroup))) {
                    this.groupService.RemoveMacroFromGroup(macro.Id);
                }
                foreach (var g in groups) {
                    if (ImGui.Selectable(g, currentGroup == g)) {
                        this.groupService.AssignMacroToGroup(macro.Id, g);
                    }
                }
                ImGui.EndCombo();
            }

            ImGui.Spacing();
            ImGui.Separator();
            ImGui.Spacing();

            ImGui.TextDisabled(this.localization.Translate("config_macro_tags"));

            var macroTags = this.tagService.GetTagsForMacro(macro.Id).ToList();

            if (ImGui.BeginTable("MacroTagsTable", 2, ImGuiTableFlags.BordersInnerH | ImGuiTableFlags.RowBg)) {
                ImGui.TableSetupColumn(this.localization.Translate("config_common_tags"), ImGuiTableColumnFlags.WidthStretch);
                ImGui.TableSetupColumn(this.localization.Translate("config_common_actions"), ImGuiTableColumnFlags.WidthFixed, 60f);

                if (macroTags.Count == 0) {
                    ImGui.TableNextRow();
                    ImGui.TableNextColumn();
                    ImGui.TextDisabled(this.localization.Translate("browser_details_no_assigned_tags"));
                    ImGui.TableNextColumn();
                }

                string? tagToRemove = null;
                foreach (var tag in macroTags) {
                    ImGui.TableNextRow();
                    ImGui.TableNextColumn();
                    ImGui.AlignTextToFramePadding();
                    ImGui.TextUnformatted(tag);

                    ImGui.TableNextColumn();
                    if (ImGui.Button($"{this.localization.Translate("browser_details_remove")}##{tag}", new Vector2(-1f, 0))) tagToRemove = tag;
                }
                ImGui.EndTable();

                if (tagToRemove != null) {
                    this.tagService.RemoveTagFromMacro(macro.Id, tagToRemove);
                }
            }

            ImGui.Spacing();

            var availableTags = this.tagService.GetAvailableTags().Except(macroTags).ToList();

            if (ImGui.BeginTable("AddMacroTagTable", 1, ImGuiTableFlags.None)) {
                ImGui.TableSetupColumn("Combo", ImGuiTableColumnFlags.WidthStretch);
                ImGui.TableNextRow();
                ImGui.TableNextColumn();

                ImGui.SetNextItemWidth(-1f);
                if (availableTags.Count > 0) {
                    if (ImGui.BeginCombo("##addTagMacroCombo", this.localization.Translate("browser_details_select_tag"))) {
                        foreach (var tag in availableTags) {
                            if (ImGui.Selectable(tag)) {
                                this.tagService.AddTagToMacro(macro.Id, tag);
                            }
                        }
                        ImGui.EndCombo();
                    }
                }
                else {
                    ImGui.BeginDisabled();
                    if (ImGui.BeginCombo("##addTagMacroCombo", this.localization.Translate("browser_details_no_available_tags"))) ImGui.EndCombo();
                    ImGui.EndDisabled();
                }
                ImGui.EndTable();
            }

            ImGui.Spacing();
            ImGui.Separator();
            ImGui.Spacing();

            ImGui.BeginDisabled(macro.IsLocked);

            ImGui.TextDisabled(this.localization.Translate("config_macro_col_content"));
            ImGui.Spacing();

            string content = macro.Content;
            float inputHeight = ImGui.GetTextLineHeight() * 10f;

            if (ImGui.InputTextMultiline("##MacroContent", ref content, 8192, new Vector2(-1, inputHeight))) {
                macro.Content = content;
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
                            macro.Content += result.Payload;
                            changed = true;
                            this.autoTranslateSearch = string.Empty;
                            this.autoTranslateResults.Clear();
                            break;
                        }
                    }
                    ImGui.EndChild();
                }
            }

            ImGui.EndDisabled();
        }
        ImGui.EndChild();

        if (changed) {
            this.macroService.UpdateMacro(macro.Id, macro.Name, macro.Content, macro.IconId, macro.IsLocked);
        }
    }

    private void DrawStaticHotbarAssignment(Guid macroId) {
        ImGui.TextDisabled(this.localization.Translate("browser_details_static_hotbars"));

        var context = this.contextService.GetCurrentContext();
        var manualHotbars = context.Hotbars.Where(h => h.PopulationMode == HotbarPopulationMode.Manual).ToList();

        if (!manualHotbars.Any()) {
            ImGui.TextDisabled(this.localization.Translate("browser_details_no_manual_hotbars"));
            return;
        }

        bool hotbarChanged = false;
        foreach (var hotbar in manualHotbars) {
            bool isInHotbar = hotbar.ManualMacroIds.Contains(macroId);
            if (ImGui.Checkbox($"{hotbar.Name}##hb_{hotbar.Id}", ref isInHotbar)) {
                if (isInHotbar) hotbar.ManualMacroIds.Add(macroId);
                else hotbar.ManualMacroIds.Remove(macroId);

                hotbarChanged = true;
            }
        }

        if (hotbarChanged) {
            this.configurationService.Save();
            this.contextService.NotifyHotbarsChanged();
        }
    }

    private void DrawIconPickerPopup(RoleplayMacro macro, ref bool changed) {
        ImGui.SetNextWindowSize(new Vector2(340, 400), ImGuiCond.Appearing);

        if (ImGui.BeginPopup("IconPickerPopup")) {
            ImGui.TextDisabled(this.localization.Translate("config_macro_icon_picker"));
            ImGui.Separator();

            if (ImGui.BeginTabBar("IconPickerTabs")) {
                if (ImGui.BeginTabItem(this.localization.Translate("config_macro_icon_tab_macros"))) {
                    this.DrawIconGrid("MacroIconsGrid", 66001, 66344, null, macro, ref changed);
                    ImGui.EndTabItem();
                }

                if (ImGui.BeginTabItem(this.localization.Translate("config_macro_icon_tab_emotes"))) {
                    var emoteIcons = this.emoteCache.GetCachedEmotes().Select(e => e.IconId).Distinct().ToList();
                    this.DrawIconGrid("EmoteIconsGrid", 0, 0, emoteIcons, macro, ref changed);
                    ImGui.EndTabItem();
                }

                ImGui.EndTabBar();
            }
            ImGui.EndPopup();
        }
    }

    private void DrawIconGrid(string id, uint startId, uint endId, List<uint>? specificIcons, RoleplayMacro macro, ref bool changed) {
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
                                macro.IconId = iconId;
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
}