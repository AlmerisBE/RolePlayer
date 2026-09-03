namespace RolePlayer.UI.MainWindow.Tabs;

using Dalamud.Bindings.ImGui;
using Dalamud.Interface;
using Dalamud.Interface.Textures;
using Dalamud.Interface.Textures.Internal;
using Dalamud.Plugin.Services;
using RolePlayer.Core.Configuration.Contracts;
using RolePlayer.Core.MetaData.Models;
using RolePlayer.UI.EmoteBrowser.Contracts;
using RolePlayer.UI.Hotbar.Components;
using RolePlayer.UI.Hotbar.Contracts;
using RolePlayer.UI.Hotbar.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

public class ConfigurationTab : IEmoteBrowserTab, IDisposable {
    public string TabName => "Configuration";
    public int SortOrder => 99;

    public bool IsSidePanelOpen => this.selectedHotbar != null && this.isHotbarTabActive;

    private IConfigurationService configService;
    private HotbarManagerComponent hotbarManager;
    private IHotbarResolverService hotbarResolver;
    private ITextureProvider textureProvider;

    private HotbarConfig? selectedHotbar;
    private bool isHotbarTabActive = true;

    private string newTagName = string.Empty;
    private string newGroupName = string.Empty;

    public ConfigurationTab(
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
        if (ImGui.BeginTabBar("ConfigurationTabBar")) {
            if (ImGui.BeginTabItem("Hotbars")) {
                this.isHotbarTabActive = true;
                this.DrawHotbarSettings();
                ImGui.EndTabItem();
            }

            if (ImGui.BeginTabItem("Groups & Tags")) {
                this.isHotbarTabActive = false;
                this.DrawGroupsAndTagsSettings();
                ImGui.EndTabItem();
            }

            ImGui.EndTabBar();
        }
    }

    private void DrawHotbarSettings() {
        var config = this.configService.GetConfig();

        ImGui.Text("Hotbar Management");
        ImGui.Separator();
        ImGui.Spacing();

        if (ImGui.Button("Create New Hotbar", new Vector2(-1, 0))) {
            var newHotbar = new HotbarConfig { Name = $"Hotbar {config.Hotbars.Count + 1}" };
            config.Hotbars.Add(newHotbar);
            this.selectedHotbar = newHotbar;
            this.configService.Save();
            this.hotbarManager.RefreshWindows();
        }

        ImGui.Spacing();

        foreach (var hotbar in config.Hotbars) {
            bool isSelected = this.selectedHotbar?.Id == hotbar.Id;
            if (ImGui.Selectable(hotbar.Name, isSelected)) {
                this.selectedHotbar = hotbar;
            }
        }
    }

    private void DrawGroupsAndTagsSettings() {
        var config = this.configService.GetConfig();
        bool configChanged = false;

        ImGui.Text("Tag Management");
        ImGui.Separator();
        ImGui.Spacing();

        ImGui.SetNextItemWidth(200f);
        ImGui.InputTextWithHint("##NewTag", "New tag name...", ref this.newTagName, 64);
        ImGui.SameLine();

        if (ImGui.Button("Add Tag") && !string.IsNullOrWhiteSpace(this.newTagName)) {
            if (config.AvailableTags.Add(this.newTagName.Trim())) {
                configChanged = true;
            }

            this.newTagName = string.Empty;
        }

        ImGui.Spacing();

        foreach (var tag in config.AvailableTags.ToList()) {
            ImGui.AlignTextToFramePadding();
            ImGui.Text(tag);
            ImGui.SameLine(250f);

            if (ImGui.Button($"Remove##Tag_{tag}")) {
                config.AvailableTags.Remove(tag);
                configChanged = true;
            }
        }

        ImGui.Spacing();
        ImGui.Spacing();
        ImGui.Text("Group Management");
        ImGui.Separator();
        ImGui.Spacing();

        ImGui.SetNextItemWidth(200f);
        ImGui.InputTextWithHint("##NewGroup", "New group name...", ref this.newGroupName, 64);
        ImGui.SameLine();

        if (ImGui.Button("Add Group") && !string.IsNullOrWhiteSpace(this.newGroupName)) {
            var groupName = this.newGroupName.Trim();
            if (!config.EmoteGroups.Any(g => g.Name.Equals(groupName, StringComparison.OrdinalIgnoreCase))) {
                config.EmoteGroups.Add(new EmoteGroup { Name = groupName });
                configChanged = true;
            }
            this.newGroupName = string.Empty;
        }

        ImGui.Spacing();

        foreach (var group in config.EmoteGroups.ToList()) {
            ImGui.AlignTextToFramePadding();
            ImGui.Text(group.Name);
            ImGui.SameLine(250f);

            if (ImGui.Button($"Remove##Group_{group.Name}")) {
                config.EmoteGroups.Remove(group);
                configChanged = true;
            }
        }

        if (configChanged) {
            this.configService.Save();
        }
    }

    public void DrawSidePanel() {
        if (!this.isHotbarTabActive || this.selectedHotbar == null) {
            return;
        }

        var config = this.configService.GetConfig();
        bool configChanged = false;

        ImGui.Text("Hotbar Settings");
        ImGui.Separator();
        ImGui.Spacing();

        // Toolbar
        ImGui.PushFont(UiBuilder.IconFont);

        var eyeIcon = this.selectedHotbar.IsVisible ? FontAwesomeIcon.Eye.ToIconString() : FontAwesomeIcon.EyeSlash.ToIconString();
        if (ImGui.Button(eyeIcon)) {
            this.selectedHotbar.IsVisible = !this.selectedHotbar.IsVisible;
            configChanged = true;
        }
        if (ImGui.IsItemHovered()) {
            ImGui.SetTooltip("Toggle Visibility");
        }

        ImGui.SameLine();

        var lockIcon = this.selectedHotbar.IsLocked ? FontAwesomeIcon.Lock.ToIconString() : FontAwesomeIcon.Unlock.ToIconString();
        if (ImGui.Button(lockIcon)) {
            this.selectedHotbar.IsLocked = !this.selectedHotbar.IsLocked;
            configChanged = true;
        }
        if (ImGui.IsItemHovered()) {
            ImGui.SetTooltip("Toggle Position Lock");
        }

        ImGui.SameLine();
        ImGui.SetCursorPosX(ImGui.GetWindowContentRegionMax().X - 30f);

        ImGui.PushStyleColor(ImGuiCol.Text, new Vector4(0.8f, 0.2f, 0.2f, 1.0f));
        if (ImGui.Button(FontAwesomeIcon.Trash.ToIconString())) {
            config.Hotbars.Remove(this.selectedHotbar);
            this.selectedHotbar = null;
            configChanged = true;
        }
        ImGui.PopStyleColor();
        if (ImGui.IsItemHovered()) {
            ImGui.SetTooltip("Delete Hotbar");
        }

        ImGui.PopFont();
        ImGui.Spacing();

        if (this.selectedHotbar == null) {
            return;
        }

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

            var groups = config.EmoteGroups.Select(g => g.Name).ToList();
            this.DrawMultiSelectCombo("Groups", groups, this.selectedHotbar.SelectedGroups, ref configChanged);

            var tags = config.AvailableTags.ToList();
            this.DrawMultiSelectCombo("Tags", tags, this.selectedHotbar.SelectedTags, ref configChanged);

            ImGui.Spacing();
            this.DrawDynamicPreview();
        }
        else {
            ImGui.Text("Manual Population (Select emotes from the browser)");
            // La logique de sélection manuelle (Drag&Drop, etc.) sera ajoutée ici
        }

        if (configChanged) {
            this.configService.Save();
            this.hotbarManager.RefreshWindows();
        }
    }

    private void DrawDynamicPreview() {
        var resolvedEmotes = this.hotbarResolver.ResolveEmotesForHotbar(this.selectedHotbar!, this.hotbarManager.GetEmoteCache());

        ImGui.Separator();
        ImGui.Spacing();
        ImGui.TextColored(new Vector4(0.7f, 0.7f, 0.7f, 1.0f), $"Preview: {resolvedEmotes.Count} matching emotes");
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

    public void Dispose() { }
}