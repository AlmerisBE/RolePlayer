namespace RolePlayer.UI.MainWindow.Tabs;

using Dalamud.Bindings.ImGui;
using RolePlayer.UI.EmoteBrowser.Contracts;
using RolePlayer.UI.MainWindow.Tabs.SubTabs;
using System;

public class ConfigurationTab : IEmoteBrowserTab, IDisposable {
    public string TabName => "Configuration";
    public int SortOrder => 99;

    public bool IsSidePanelOpen => this.isHotbarTabActive && this.hotbarConfigSubTab.IsSidePanelOpen;

    private GeneralConfigSubTab generalConfigSubTab;
    private HotbarConfigSubTab hotbarConfigSubTab;
    private GroupsConfigSubTab groupsConfigSubTab;
    private TagsConfigSubTab tagsConfigSubTab;
    private ContextsConfigSubTab contextsConfigSubTab;

    private bool isHotbarTabActive = true;

    public ConfigurationTab(
        GeneralConfigSubTab generalConfigSubTab,
        HotbarConfigSubTab hotbarConfigSubTab,
        GroupsConfigSubTab groupsConfigSubTab,
        TagsConfigSubTab tagsConfigSubTab,
        ContextsConfigSubTab contextsConfigSubTab) {

        this.generalConfigSubTab = generalConfigSubTab;
        this.hotbarConfigSubTab = hotbarConfigSubTab;
        this.groupsConfigSubTab = groupsConfigSubTab;
        this.tagsConfigSubTab = tagsConfigSubTab;
        this.contextsConfigSubTab = contextsConfigSubTab;
    }

    public void Draw() {
        if (ImGui.BeginTabBar("ConfigurationTabBar")) {
            if (ImGui.BeginTabItem("General")) {
                this.isHotbarTabActive = false;
                this.generalConfigSubTab.Draw();
                ImGui.EndTabItem();
            }

            if (ImGui.BeginTabItem("Contexts")) {
                this.isHotbarTabActive = false;
                this.contextsConfigSubTab.Draw();
                ImGui.EndTabItem();
            }

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