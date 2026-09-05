namespace RolePlayer.UI.MainWindow.Tabs.SubTabs;

using Dalamud.Bindings.ImGui;
using Dalamud.Interface;
using Dalamud.Interface.Textures;
using Dalamud.Interface.Textures.Internal;
using Dalamud.Plugin.Services;
using RolePlayer.Core.Configuration.Contracts;
using RolePlayer.UI.Hotbar.Components;
using RolePlayer.UI.Hotbar.Contracts;
using RolePlayer.UI.Hotbar.Models;
using RolePlayer.UI.Localization.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

public class HotbarConfigSubTab {
    private IConfigurationService configService;
    private IContextManagementService contextService;
    private HotbarManagerComponent hotbarManager;
    private IHotbarResolverService hotbarResolver;
    private ITextureProvider textureProvider;
    private ILocalizationService localization;

    private HotbarConfig? selectedHotbar;
    private HotbarConfig? hotbarToDelete;
    private bool isDeleteDialogOpen = false;

    public bool IsSidePanelOpen => this.selectedHotbar != null;

    public HotbarConfigSubTab(
        IConfigurationService configService,
        IContextManagementService contextService,
        HotbarManagerComponent hotbarManager,
        IHotbarResolverService hotbarResolver,
        ITextureProvider textureProvider,
        ILocalizationService localization) {

        this.configService = configService;
        this.contextService = contextService;
        this.hotbarManager = hotbarManager;
        this.hotbarResolver = hotbarResolver;
        this.textureProvider = textureProvider;
        this.localization = localization;
    }

    public void Draw() {
        var context = this.contextService.GetCurrentContext();

        ImGui.Text(this.localization.Translate("config_hb_manage"));
        ImGui.Separator();
        ImGui.Spacing();

        ImGui.PushFont(UiBuilder.IconFont);
        bool addClicked = ImGui.Button(FontAwesomeIcon.Plus.ToIconString());
        ImGui.PopFont();

        ImGui.SameLine();
        ImGui.AlignTextToFramePadding();
        ImGui.Text(this.localization.Translate("config_hb_create"));

        if (addClicked) {
            var newHotbar = new HotbarConfig { Name = $"Hotbar {context.Hotbars.Count + 1}" };
            context.Hotbars.Add(newHotbar);
            this.selectedHotbar = newHotbar;
            this.configService.Save();
            this.hotbarManager.RefreshWindows();
        }

        ImGui.Spacing();

        if (context.Hotbars.Count == 0) {
            ImGui.TextDisabled(this.localization.Translate("config_hb_no_hotbars"));
            return;
        }

        if (ImGui.BeginTable("HotbarsListTable", 3, ImGuiTableFlags.Borders | ImGuiTableFlags.RowBg | ImGuiTableFlags.SizingFixedFit)) {
            ImGui.TableSetupColumn(this.localization.Translate("config_common_name"), ImGuiTableColumnFlags.WidthStretch);
            ImGui.TableSetupColumn(this.localization.Translate("config_hb_table_type"), ImGuiTableColumnFlags.WidthFixed, 100f);
            ImGui.TableSetupColumn(this.localization.Translate("config_hb_table_emotes"), ImGuiTableColumnFlags.WidthFixed, 60f);
            ImGui.TableHeadersRow();

            foreach (var hotbar in context.Hotbars) {
                ImGui.TableNextRow();

                bool isSelected = this.selectedHotbar?.Id == hotbar.Id;

                ImGui.TableNextColumn();
                if (ImGui.Selectable($"{hotbar.Name}##sel_{hotbar.Id}", isSelected, ImGuiSelectableFlags.SpanAllColumns | ImGuiSelectableFlags.AllowItemOverlap)) this.selectedHotbar = hotbar;

                ImGui.TableNextColumn();
                string popModeStr = hotbar.PopulationMode == HotbarPopulationMode.Manual ? this.localization.Translate("config_hb_pop_manual") : this.localization.Translate("config_hb_pop_dynamic");
                ImGui.Text(popModeStr);

                ImGui.TableNextColumn();
                int count = this.hotbarResolver.ResolveEmotesForHotbar(hotbar, this.hotbarManager.GetEmoteCache()).Count;
                ImGui.Text(count.ToString());
            }
            ImGui.EndTable();
        }
    }

    public void DrawSidePanel() {
        if (this.selectedHotbar == null) return;

        var context = this.contextService.GetCurrentContext();
        bool configChanged = false;

        string closeIcon = FontAwesomeIcon.Times.ToIconString();
        ImGui.PushFont(UiBuilder.IconFont);
        var closeBtnWidth = ImGui.CalcTextSize(closeIcon).X + ImGui.GetStyle().FramePadding.X * 2;
        ImGui.PopFont();

        if (ImGui.BeginTable("HotbarSettingsHeaderTable", 2)) {
            ImGui.TableSetupColumn("Title", ImGuiTableColumnFlags.WidthStretch);
            ImGui.TableSetupColumn("CloseBtn", ImGuiTableColumnFlags.WidthFixed, closeBtnWidth);

            ImGui.TableNextRow();
            ImGui.TableNextColumn();
            ImGui.AlignTextToFramePadding();
            ImGui.SetWindowFontScale(1.3f);

            string title = string.IsNullOrWhiteSpace(this.selectedHotbar.Name) ? this.localization.Translate("config_hb_settings") : this.selectedHotbar.Name;
            ImGui.TextUnformatted(title);
            ImGui.SetWindowFontScale(1.0f);

            ImGui.TableNextColumn();
            ImGui.PushFont(UiBuilder.IconFont);
            if (ImGui.Button($"{closeIcon}##CloseHotbarDetails")) {
                this.selectedHotbar = null;
                ImGui.PopFont();
                ImGui.EndTable();
                return;
            }
            ImGui.PopFont();

            ImGui.EndTable();
        }

        ImGui.Separator();
        ImGui.Spacing();

        var eyeIcon = this.selectedHotbar.IsVisible ? FontAwesomeIcon.Eye.ToIconString() : FontAwesomeIcon.EyeSlash.ToIconString();
        ImGui.PushFont(UiBuilder.IconFont);
        bool toggleVis = ImGui.Button(eyeIcon);
        ImGui.PopFont();

        if (toggleVis) {
            this.selectedHotbar.IsVisible = !this.selectedHotbar.IsVisible;
            configChanged = true;
        }
        if (ImGui.IsItemHovered()) ImGui.SetTooltip(this.localization.Translate("config_hb_tooltip_vis"));

        ImGui.SameLine();

        var lockIcon = this.selectedHotbar.IsLocked ? FontAwesomeIcon.Lock.ToIconString() : FontAwesomeIcon.Unlock.ToIconString();
        ImGui.PushFont(UiBuilder.IconFont);
        bool toggleLock = ImGui.Button(lockIcon);
        ImGui.PopFont();

        if (toggleLock) {
            this.selectedHotbar.IsLocked = !this.selectedHotbar.IsLocked;
            configChanged = true;
        }
        if (ImGui.IsItemHovered()) ImGui.SetTooltip(this.localization.Translate("config_hb_tooltip_lock"));

        ImGui.SameLine();
        ImGui.SetCursorPosX(ImGui.GetWindowContentRegionMax().X - 30f);

        ImGui.PushStyleColor(ImGuiCol.Text, new Vector4(0.8f, 0.2f, 0.2f, 1.0f));
        ImGui.PushFont(UiBuilder.IconFont);
        bool doDelete = ImGui.Button(FontAwesomeIcon.Trash.ToIconString());
        ImGui.PopFont();
        ImGui.PopStyleColor();

        if (doDelete) {
            this.hotbarToDelete = this.selectedHotbar;
            this.isDeleteDialogOpen = true;
        }
        if (ImGui.IsItemHovered()) ImGui.SetTooltip(this.localization.Translate("config_hb_tooltip_del"));

        ImGui.Spacing();

        ImGui.TextDisabled(this.localization.Translate("config_hb_auto_hide"));

        bool hideCombat = this.selectedHotbar.HideInCombat;
        if (ImGui.Checkbox($"{this.localization.Translate("config_hb_combat")}##hideCombat", ref hideCombat)) {
            this.selectedHotbar.HideInCombat = hideCombat;
            configChanged = true;
        }

        ImGui.SameLine();

        bool hideDuty = this.selectedHotbar.HideInDuty;
        if (ImGui.Checkbox($"{this.localization.Translate("config_hb_duty")}##hideDuty", ref hideDuty)) {
            this.selectedHotbar.HideInDuty = hideDuty;
            configChanged = true;
        }

        ImGui.Spacing();
        ImGui.Separator();
        ImGui.Spacing();

        string name = this.selectedHotbar.Name;
        if (ImGui.InputText(this.localization.Translate("config_common_name"), ref name, 64)) {
            this.selectedHotbar.Name = name;
            configChanged = true;
        }

        if (ImGui.BeginCombo(this.localization.Translate("config_hb_layout"), this.selectedHotbar.Layout.ToString())) {
            foreach (HotbarLayout layout in Enum.GetValues(typeof(HotbarLayout))) {
                if (ImGui.Selectable(layout.ToString(), this.selectedHotbar.Layout == layout)) {
                    this.selectedHotbar.Layout = layout;
                    configChanged = true;
                }
            }
            ImGui.EndCombo();
        }

        if (ImGui.BeginCombo(this.localization.Translate("config_hb_anchor"), this.selectedHotbar.Anchor.ToString())) {
            foreach (HotbarAnchor anchor in Enum.GetValues(typeof(HotbarAnchor))) {
                if (ImGui.Selectable(anchor.ToString(), this.selectedHotbar.Anchor == anchor)) {
                    this.selectedHotbar.Anchor = anchor;
                    this.selectedHotbar.PositionInitialized = false;
                    configChanged = true;
                }
            }
            ImGui.EndCombo();
        }

        string currentPopModeStr = this.selectedHotbar.PopulationMode == HotbarPopulationMode.Manual ? this.localization.Translate("config_hb_pop_manual") : this.localization.Translate("config_hb_pop_dynamic");

        if (ImGui.BeginCombo(this.localization.Translate("config_hb_pop_mode"), currentPopModeStr)) {
            foreach (HotbarPopulationMode mode in Enum.GetValues(typeof(HotbarPopulationMode))) {
                string modeStr = mode == HotbarPopulationMode.Manual ? this.localization.Translate("config_hb_pop_manual") : this.localization.Translate("config_hb_pop_dynamic");

                if (ImGui.Selectable(modeStr, this.selectedHotbar.PopulationMode == mode)) {
                    this.selectedHotbar.PopulationMode = mode;
                    configChanged = true;
                }
            }
            ImGui.EndCombo();
        }

        ImGui.Spacing();
        float scalePercent = this.selectedHotbar.Scale * 100f;
        if (ImGui.SliderFloat(this.localization.Translate("config_hb_scale"), ref scalePercent, 75f, 125f, "%.0f%%")) {
            this.selectedHotbar.Scale = scalePercent / 100f;
            configChanged = true;
        }

        ImGui.Spacing();
        ImGui.Separator();
        ImGui.Spacing();

        if (this.selectedHotbar.PopulationMode == HotbarPopulationMode.Dynamic) {
            ImGui.Text(this.localization.Translate("config_hb_dyn_filters"));
            string searchQuery = this.selectedHotbar.SearchQuery;
            if (ImGui.InputTextWithHint("##HotbarSearch", this.localization.Translate("config_hb_search"), ref searchQuery, 128)) {
                this.selectedHotbar.SearchQuery = searchQuery;
                configChanged = true;
            }

            bool moddedOnly = this.selectedHotbar.ShowModdedOnly;
            if (ImGui.Checkbox(this.localization.Translate("config_hb_modded_only"), ref moddedOnly)) {
                this.selectedHotbar.ShowModdedOnly = moddedOnly;
                configChanged = true;
            }

            ImGui.Spacing();

            var categories = this.hotbarManager.GetEmoteCache().Select(e => e.Category).Where(c => !string.IsNullOrEmpty(c)).Distinct().ToList();
            this.DrawMultiSelectCombo(this.localization.Translate("config_common_categories"), categories, this.selectedHotbar.SelectedCategories, ref configChanged);

            var groups = context.EmoteGroups.Select(g => g.Name).ToList();
            this.DrawMultiSelectCombo(this.localization.Translate("config_common_groups"), groups, this.selectedHotbar.SelectedGroups, ref configChanged);

            var tags = context.AvailableTags.ToList();
            this.DrawMultiSelectCombo(this.localization.Translate("config_common_tags"), tags, this.selectedHotbar.SelectedTags, ref configChanged);
        }
        else {
            ImGui.Text(this.localization.Translate("config_hb_manual_pop"));
            ImGui.TextDisabled(this.localization.Translate("config_hb_manual_desc"));
        }

        ImGui.Spacing();

        this.DrawEmotePreview();

        if (configChanged) {
            this.configService.Save();
            this.hotbarManager.RefreshWindows();
        }

        this.DrawDeleteConfirmationModal();
    }

    private void DrawDeleteConfirmationModal() {
        if (this.isDeleteDialogOpen) {
            ImGui.OpenPopup(this.localization.Translate("config_hb_del_title"));
            this.isDeleteDialogOpen = false;
        }

        if (ImGui.BeginPopupModal(this.localization.Translate("config_hb_del_title"), ImGuiWindowFlags.AlwaysAutoResize | ImGuiWindowFlags.NoSavedSettings)) {
            ImGui.Text(this.localization.Translate("config_hb_del_desc", this.hotbarToDelete?.Name ?? "Unknown"));
            ImGui.TextColored(new Vector4(0.8f, 0.2f, 0.2f, 1.0f), this.localization.Translate("config_hb_del_warn"));
            ImGui.Spacing();
            ImGui.Separator();
            ImGui.Spacing();

            if (ImGui.Button(this.localization.Translate("config_common_yes_delete"), new Vector2(120, 0))) {
                if (this.hotbarToDelete != null) {
                    var context = this.contextService.GetCurrentContext();
                    context.Hotbars.Remove(this.hotbarToDelete);
                    if (this.selectedHotbar == this.hotbarToDelete) this.selectedHotbar = null;

                    this.configService.Save();
                    this.hotbarManager.RefreshWindows();
                }
                ImGui.CloseCurrentPopup();
            }
            ImGui.SameLine();
            if (ImGui.Button(this.localization.Translate("config_common_cancel"), new Vector2(120, 0))) ImGui.CloseCurrentPopup();

            ImGui.EndPopup();
        }
    }

    private void DrawEmotePreview() {
        var resolvedEmotes = this.hotbarResolver.ResolveEmotesForHotbar(this.selectedHotbar!, this.hotbarManager.GetEmoteCache());

        ImGui.Separator();
        ImGui.Spacing();
        ImGui.TextColored(new Vector4(0.7f, 0.7f, 0.7f, 1.0f), this.localization.Translate("config_hb_preview", resolvedEmotes.Count));
        ImGui.Spacing();

        int totalEmotes = resolvedEmotes.Count;
        if (totalEmotes == 0) return;

        float availWidth = ImGui.GetContentRegionAvail().X;
        int cols = (int)(availWidth / 36f);
        if (cols < 1) cols = 1;

        if (ImGui.BeginTable("PreviewGrid", cols, ImGuiTableFlags.SizingFixedFit)) {
            for (int i = 0; i < totalEmotes; i++) {
                if (i % cols == 0) ImGui.TableNextRow();

                ImGui.TableNextColumn();

                var emote = resolvedEmotes[i];
                if (emote.IconId > 0) {
                    try {
                        var lookup = new GameIconLookup { IconId = emote.IconId, HiRes = false };
                        var iconWrap = this.textureProvider.GetFromGameIcon(lookup).GetWrapOrDefault();

                        if (iconWrap != null) {
                            ImGui.Image(iconWrap.Handle, new Vector2(32, 32));
                            if (ImGui.IsItemHovered()) ImGui.SetTooltip(emote.Name);
                        }
                    }
                    catch (IconNotFoundException) { }
                }
            }
            ImGui.EndTable();
        }
    }

    private void DrawMultiSelectCombo(string label, List<string> items, HashSet<string> selectedItems, ref bool changed) {
        var preview = selectedItems.Count == 0 ? this.localization.Translate("config_common_all") : $"{selectedItems.Count} {this.localization.Translate("config_common_selected")}";

        if (ImGui.BeginCombo(label, preview)) {
            bool allSelected = selectedItems.Count == 0;
            if (ImGui.Checkbox(this.localization.Translate("config_common_all"), ref allSelected)) {
                selectedItems.Clear();
                changed = true;
            }

            ImGui.Separator();

            foreach (var item in items) {
                bool isSelected = selectedItems.Contains(item);
                if (ImGui.Checkbox(item, ref isSelected)) {
                    if (isSelected) selectedItems.Add(item);
                    else selectedItems.Remove(item);

                    changed = true;
                }
            }
            ImGui.EndCombo();
        }
    }
}