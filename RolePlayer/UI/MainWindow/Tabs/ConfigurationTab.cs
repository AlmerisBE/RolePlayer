namespace RolePlayer.UI.MainWindow.Tabs;

using Dalamud.Bindings.ImGui;
using RolePlayer.UI.EmoteBrowser.Contracts;
using RolePlayer.UI.Localization.Contracts;
using RolePlayer.UI.MainWindow.Tabs.SubTabs;
using System;

public class ConfigurationTab : IEmoteBrowserTab, IDisposable {
    private ILocalizationService localization;

    public string TabName => this.localization.Translate("config_tab_config");
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
        ContextsConfigSubTab contextsConfigSubTab,
        ILocalizationService localization) {

        this.generalConfigSubTab = generalConfigSubTab;
        this.hotbarConfigSubTab = hotbarConfigSubTab;
        this.groupsConfigSubTab = groupsConfigSubTab;
        this.tagsConfigSubTab = tagsConfigSubTab;
        this.contextsConfigSubTab = contextsConfigSubTab;
        this.localization = localization;
    }

    public void Draw() {
        if (ImGui.BeginTabBar("ConfigurationTabBar")) {
            if (ImGui.BeginTabItem(this.localization.Translate("config_tab_general"))) {
                this.isHotbarTabActive = false;
                this.generalConfigSubTab.Draw();
                ImGui.EndTabItem();
            }

            if (ImGui.BeginTabItem(this.localization.Translate("config_tab_contexts"))) {
                this.isHotbarTabActive = false;
                this.contextsConfigSubTab.Draw();
                ImGui.EndTabItem();
            }

            if (ImGui.BeginTabItem(this.localization.Translate("config_tab_hotbars"))) {
                this.isHotbarTabActive = true;
                this.hotbarConfigSubTab.Draw();
                ImGui.EndTabItem();
            }

            if (ImGui.BeginTabItem(this.localization.Translate("config_tab_groups"))) {
                this.isHotbarTabActive = false;
                this.groupsConfigSubTab.Draw();
                ImGui.EndTabItem();
            }

            if (ImGui.BeginTabItem(this.localization.Translate("config_tab_tags"))) {
                this.isHotbarTabActive = false;
                this.tagsConfigSubTab.Draw();
                ImGui.EndTabItem();
            }

            ImGui.EndTabBar();
        }
    }

    public void DrawSidePanel() {
        if (this.isHotbarTabActive) this.hotbarConfigSubTab.DrawSidePanel();
    }

    public void Dispose() { }
}