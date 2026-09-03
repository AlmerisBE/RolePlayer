namespace RolePlayer.UI.MainWindow.Tabs;

using Dalamud.Bindings.ImGui;
using RolePlayer.Core.Configuration.Contracts;
using RolePlayer.UI.EmoteBrowser.Contracts;
using RolePlayer.UI.Hotbar.Components;
using RolePlayer.UI.Hotbar.Models;
using System;
using System.Numerics;

public class ConfigurationTab : IEmoteBrowserTab, IDisposable {
    public string TabName => "Configuration";
    public int SortOrder => 99;

    public bool IsSidePanelOpen => this.selectedHotbar != null;

    private IConfigurationService configService;
    private HotbarManagerComponent hotbarManager;
    private HotbarConfig? selectedHotbar;

    public ConfigurationTab(IConfigurationService configService, HotbarManagerComponent hotbarManager) {
        this.configService = configService;
        this.hotbarManager = hotbarManager;
    }

    public void Draw() {
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

    public void DrawSidePanel() {
        if (this.selectedHotbar == null) {
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