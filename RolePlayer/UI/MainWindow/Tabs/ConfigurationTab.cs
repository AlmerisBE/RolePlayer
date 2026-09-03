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
    private GroupsConfigSubTab groupsConfigSubTab;
    private TagsConfigSubTab tagsConfigSubTab;

    private bool isHotbarTabActive = true;

    public ConfigurationTab(
        HotbarConfigSubTab hotbarConfigSubTab,
        GroupsConfigSubTab groupsConfigSubTab,
        TagsConfigSubTab tagsConfigSubTab) {

        this.hotbarConfigSubTab = hotbarConfigSubTab;
        this.groupsConfigSubTab = groupsConfigSubTab;
        this.tagsConfigSubTab = tagsConfigSubTab;
    }

    public void Draw() {
        if (ImGui.BeginTabBar("ConfigurationTabBar")) {
            if (ImGui.BeginTabItem("Hotbars")) {
                this.isHotbarTabActive = true;
                this.hotbarConfigSubTab.Draw();
                ImGui.EndTabItem();
            }

            if (ImGui.BeginTabItem("Groups")) {
                this.isHotbarTabActive = false;
                this.groupsConfigSubTab.Draw();
                ImGui.EndTabItem();
            }

            if (ImGui.BeginTabItem("Tags")) {
                this.isHotbarTabActive = false;
                this.tagsConfigSubTab.Draw();
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