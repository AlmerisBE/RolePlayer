namespace RolePlayer.UI.MainWindow.Tabs;

using Dalamud.Bindings.ImGui;
using RolePlayer.UI.EmoteBrowser.Contracts;
using RolePlayer.UI.MainWindow.Tabs.SubTabs;
using System;

public class ConfigurationTab : IEmoteBrowserTab, IDisposable {
    public string TabName => "Configuration";
    public int SortOrder => 99;

    public bool IsSidePanelOpen => this.isHotbarTabActive && this.hotbarConfigSubTab.IsSidePanelOpen;

    private HotbarConfigSubTab hotbarConfigSubTab;
    private TagsGroupsConfigSubTab tagsGroupsConfigSubTab;

    private bool isHotbarTabActive = true;

    public ConfigurationTab(
        HotbarConfigSubTab hotbarConfigSubTab,
        TagsGroupsConfigSubTab tagsGroupsConfigSubTab) {

        this.hotbarConfigSubTab = hotbarConfigSubTab;
        this.tagsGroupsConfigSubTab = tagsGroupsConfigSubTab;
    }

    public void Draw() {
        if (ImGui.BeginTabBar("ConfigurationTabBar")) {
            if (ImGui.BeginTabItem("Hotbars")) {
                this.isHotbarTabActive = true;
                this.hotbarConfigSubTab.Draw();
                ImGui.EndTabItem();
            }

            if (ImGui.BeginTabItem("Groups & Tags")) {
                this.isHotbarTabActive = false;
                this.tagsGroupsConfigSubTab.Draw();
                ImGui.EndTabItem();
            }

            ImGui.EndTabBar();
        }
    }

    public void DrawSidePanel() {
        if (this.isHotbarTabActive) {
            this.hotbarConfigSubTab.DrawSidePanel();
        }
    }

    public void Dispose() { }
}