namespace RolePlayer.UI.MainWindow.Tabs;

using Dalamud.Bindings.ImGui;
using RolePlayer.Core.Configuration.Contracts;
using RolePlayer.Core.MetaData.Models;
using RolePlayer.UI.EmoteBrowser.Contracts;
using RolePlayer.UI.Hotbar.Components;
using RolePlayer.UI.Hotbar.Models;
using System;
using System.Linq;
using System.Numerics;

public class ConfigurationTab : IEmoteBrowserTab, IDisposable {
    public string TabName => "Configuration";
    public int SortOrder => 99;

    // The side panel should only open if a hotbar is selected AND we are currently on the Hotbars sub-tab
    public bool IsSidePanelOpen => this.selectedHotbar != null && this.isHotbarTabActive;

    private IConfigurationService configService;
    private HotbarManagerComponent hotbarManager;

    private HotbarConfig? selectedHotbar;
    private bool isHotbarTabActive = true;

    private string newTagName = string.Empty;
    private string newGroupName = string.Empty;

    public ConfigurationTab(IConfigurationService configService, HotbarManagerComponent hotbarManager) {
        this.configService = configService;
        this.hotbarManager = hotbarManager;
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

        string name = this.selectedHotbar.Name;
        if (ImGui.InputText("Name", ref name, 64)) {
            this.selectedHotbar.Name = name;
            configChanged = true;
        }

        bool isVisible = this.selectedHotbar.IsVisible;
        if (ImGui.Checkbox("Visible", ref isVisible)) {
            this.selectedHotbar.IsVisible = isVisible;
            configChanged = true;
        }

        ImGui.SameLine();

        bool isLocked = this.selectedHotbar.IsLocked;
        if (ImGui.Checkbox("Lock Position", ref isLocked)) {
            this.selectedHotbar.IsLocked = isLocked;
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
        }
        else {
            ImGui.Text("Manual Population (Select emotes from the browser)");
        }

        ImGui.Spacing();
        ImGui.Separator();
        ImGui.Spacing();

        ImGui.PushStyleColor(ImGuiCol.Button, new Vector4(0.8f, 0.2f, 0.2f, 1.0f));
        if (ImGui.Button("Delete Hotbar", new Vector2(-1, 0))) {
            config.Hotbars.Remove(this.selectedHotbar);
            this.selectedHotbar = null;
            configChanged = true;
        }
        ImGui.PopStyleColor();

        if (configChanged) {
            this.configService.Save();
            this.hotbarManager.RefreshWindows();
        }
    }

    public void Dispose() { }
}