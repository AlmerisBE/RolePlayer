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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

public class HotbarConfigSubTab {
    private IConfigurationService configService;
    private HotbarManagerComponent hotbarManager;
    private IHotbarResolverService hotbarResolver;
    private ITextureProvider textureProvider;

    private HotbarConfig? selectedHotbar;
    private HotbarConfig? hotbarToDelete;
    private bool isDeleteDialogOpen = false;

    public bool IsSidePanelOpen => this.selectedHotbar != null;

    public HotbarConfigSubTab(
        IConfigurationService configService,
        HotbarManagerComponent hotbarManager,
        IHotbarResolverService hotbarResolver,
        ITextureProvider textureProvider) {

        this.configService = configService;
        this.hotbarManager = hotbarManager;
        this.hotbarResolver = hotbarResolver;
        this.textureProvider = textureProvider;
    }

    public void Draw() {
        var profile = this.configService.GetCurrentProfile();

        ImGui.Text("Hotbar Management");
        ImGui.Separator();
        ImGui.Spacing();

        if (ImGui.Button("Create New Hotbar", new Vector2(-1, 0))) {
            var newHotbar = new HotbarConfig { Name = $"Hotbar {profile.Hotbars.Count + 1}" };
            profile.Hotbars.Add(newHotbar);
            this.selectedHotbar = newHotbar;
            this.configService.Save();
            this.hotbarManager.RefreshWindows();
        }

        ImGui.Spacing();

        if (profile.Hotbars.Count == 0) {
            ImGui.TextDisabled("No hotbars created yet.");
            return;
        }

        if (ImGui.BeginTable("HotbarsListTable", 3, ImGuiTableFlags.Borders | ImGuiTableFlags.RowBg | ImGuiTableFlags.SizingFixedFit)) {
            ImGui.TableSetupColumn("Name", ImGuiTableColumnFlags.WidthStretch);
            ImGui.TableSetupColumn("Type", ImGuiTableColumnFlags.WidthFixed, 100f);
            ImGui.TableSetupColumn("Emotes", ImGuiTableColumnFlags.WidthFixed, 60f);
            ImGui.TableHeadersRow();

            foreach (var hotbar in profile.Hotbars) {
                ImGui.TableNextRow();

                bool isSelected = this.selectedHotbar?.Id == hotbar.Id;

                ImGui.TableNextColumn();
                if (ImGui.Selectable($"{hotbar.Name}##sel_{hotbar.Id}", isSelected, ImGuiSelectableFlags.SpanAllColumns | ImGuiSelectableFlags.AllowItemOverlap)) {
                    this.selectedHotbar = hotbar;
                }

                ImGui.TableNextColumn();
                ImGui.Text(hotbar.PopulationMode.ToString());

                ImGui.TableNextColumn();
                int count = this.hotbarResolver.ResolveEmotesForHotbar(hotbar, this.hotbarManager.GetEmoteCache()).Count;
                ImGui.Text(count.ToString());
            }
            ImGui.EndTable();
        }
    }

    public void DrawSidePanel() {
        if (this.selectedHotbar == null) {
            return;
        }

        var profile = this.configService.GetCurrentProfile();
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
            ImGui.TextUnformatted("Hotbar Settings");
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
        if (ImGui.IsItemHovered()) {
            ImGui.SetTooltip("Toggle Visibility");
        }

        ImGui.SameLine();

        var lockIcon = this.selectedHotbar.IsLocked ? FontAwesomeIcon.Lock.ToIconString() : FontAwesomeIcon.Unlock.ToIconString();
        ImGui.PushFont(UiBuilder.IconFont);
        bool toggleLock = ImGui.Button(lockIcon);
        ImGui.PopFont();

        if (toggleLock) {
            this.selectedHotbar.IsLocked = !this.selectedHotbar.IsLocked;
            configChanged = true;
        }
        if (ImGui.IsItemHovered()) {
            ImGui.SetTooltip("Toggle Position Lock");
        }

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
        if (ImGui.IsItemHovered()) {
            ImGui.SetTooltip("Delete Hotbar");
        }

        ImGui.Spacing();

        string name = this.selectedHotbar.Name;
        if (ImGui.InputText("Name", ref name, 64)) {
            this.selectedHotbar.Name = name;
            configChanged = true;
        }

        if (ImGui.BeginCombo("Layout", this.selectedHotbar.Layout.ToString())) {
            foreach (HotbarLayout layout in Enum.GetValues(typeof(HotbarLayout))) {
                if (ImGui.Selectable(layout.ToString(), this.selectedHotbar.Layout == layout)) {
                    this.selectedHotbar.Layout = layout;
                    configChanged = true;
                }
            }
            ImGui.EndCombo();
        }

        if (ImGui.BeginCombo("Population Mode", this.selectedHotbar.PopulationMode.ToString())) {
            foreach (HotbarPopulationMode mode in Enum.GetValues(typeof(HotbarPopulationMode))) {
                if (ImGui.Selectable(mode.ToString(), this.selectedHotbar.PopulationMode == mode)) {
                    this.selectedHotbar.PopulationMode = mode;
                    configChanged = true;
                }
            }
            ImGui.EndCombo();
        }

        ImGui.Spacing();
        ImGui.Separator();
        ImGui.Spacing();

        if (this.selectedHotbar.PopulationMode == HotbarPopulationMode.Dynamic) {
            ImGui.Text("Dynamic Filters");
            string searchQuery = this.selectedHotbar.SearchQuery;
            if (ImGui.InputTextWithHint("##HotbarSearch", "Search emotes...", ref searchQuery, 128)) {
                this.selectedHotbar.SearchQuery = searchQuery;
                configChanged = true;
            }

            bool moddedOnly = this.selectedHotbar.ShowModdedOnly;
            if (ImGui.Checkbox("Modded Only", ref moddedOnly)) {
                this.selectedHotbar.ShowModdedOnly = moddedOnly;
                configChanged = true;
            }

            ImGui.Spacing();

            var categories = this.hotbarManager.GetEmoteCache().Select(e => e.Category).Where(c => !string.IsNullOrEmpty(c)).Distinct().ToList();
            this.DrawMultiSelectCombo("Categories", categories, this.selectedHotbar.SelectedCategories, ref configChanged);

            var groups = profile.EmoteGroups.Select(g => g.Name).ToList();
            this.DrawMultiSelectCombo("Groups", groups, this.selectedHotbar.SelectedGroups, ref configChanged);

            var tags = profile.AvailableTags.ToList();
            this.DrawMultiSelectCombo("Tags", tags, this.selectedHotbar.SelectedTags, ref configChanged);
        }
        else {
            ImGui.Text("Manual Population");
            ImGui.TextDisabled("Select emotes from the browser.");
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
            ImGui.OpenPopup("Delete Hotbar Confirmation");
            this.isDeleteDialogOpen = false;
        }

        if (ImGui.BeginPopupModal("Delete Hotbar Confirmation", ImGuiWindowFlags.AlwaysAutoResize | ImGuiWindowFlags.NoSavedSettings)) {
            ImGui.Text($"Are you sure you want to delete the hotbar '{this.hotbarToDelete?.Name}'?");
            ImGui.TextColored(new Vector4(0.8f, 0.2f, 0.2f, 1.0f), "This action cannot be undone.");
            ImGui.Spacing();
            ImGui.Separator();
            ImGui.Spacing();

            if (ImGui.Button("Yes, Delete", new Vector2(120, 0))) {
                if (this.hotbarToDelete != null) {
                    var profile = this.configService.GetCurrentProfile();
                    profile.Hotbars.Remove(this.hotbarToDelete);
                    if (this.selectedHotbar == this.hotbarToDelete) {
                        this.selectedHotbar = null;
                    }

                    this.configService.Save();
                    this.hotbarManager.RefreshWindows();
                }
                ImGui.CloseCurrentPopup();
            }
            ImGui.SameLine();
            if (ImGui.Button("Cancel", new Vector2(120, 0))) {
                ImGui.CloseCurrentPopup();
            }

            ImGui.EndPopup();
        }
    }

    private void DrawEmotePreview() {
        var resolvedEmotes = this.hotbarResolver.ResolveEmotesForHotbar(this.selectedHotbar!, this.hotbarManager.GetEmoteCache());

        ImGui.Separator();
        ImGui.Spacing();
        ImGui.TextColored(new Vector4(0.7f, 0.7f, 0.7f, 1.0f), $"Preview: {resolvedEmotes.Count} emotes");
        ImGui.Spacing();

        if (resolvedEmotes.Count == 0) {
            return;
        }

        int maxPreview = Math.Min(16, resolvedEmotes.Count);

        if (ImGui.BeginTable("PreviewGrid", 4, ImGuiTableFlags.SizingFixedFit)) {
            for (int i = 0; i < maxPreview; i++) {
                if (i % 4 == 0) {
                    ImGui.TableNextRow();
                }

                ImGui.TableNextColumn();

                var emote = resolvedEmotes[i];
                if (emote.IconId > 0) {
                    try {
                        var lookup = new GameIconLookup { IconId = emote.IconId, HiRes = false };
                        var iconWrap = this.textureProvider.GetFromGameIcon(lookup).GetWrapOrDefault();

                        if (iconWrap != null) {
                            ImGui.Image(iconWrap.Handle, new Vector2(32, 32));
                            if (ImGui.IsItemHovered()) {
                                ImGui.SetTooltip(emote.Name);
                            }
                        }
                    }
                    catch (IconNotFoundException) { }
                }
            }
            ImGui.EndTable();
        }
    }

    private void DrawMultiSelectCombo(string label, List<string> items, HashSet<string> selectedItems, ref bool changed) {
        var preview = selectedItems.Count == 0 ? "All" : $"{selectedItems.Count} selected";

        if (ImGui.BeginCombo(label, preview)) {
            bool allSelected = selectedItems.Count == 0;
            if (ImGui.Checkbox("All", ref allSelected)) {
                selectedItems.Clear();
                changed = true;
            }

            ImGui.Separator();

            foreach (var item in items) {
                bool isSelected = selectedItems.Contains(item);
                if (ImGui.Checkbox(item, ref isSelected)) {
                    if (isSelected) {
                        selectedItems.Add(item);
                    }
                    else {
                        selectedItems.Remove(item);
                    }

                    changed = true;
                }
            }
            ImGui.EndCombo();
        }
    }
}