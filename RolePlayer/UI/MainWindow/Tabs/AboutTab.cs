namespace RolePlayer.UI.MainWindow.Tabs;

using Dalamud.Bindings.ImGui;
using Dalamud.Interface;
using Dalamud.Plugin;
using Dalamud.Utility;
using RolePlayer.UI.EmoteBrowser.Contracts;
using RolePlayer.UI.Localization.Contracts;
using System;

public class AboutTab : IEmoteBrowserTab, IDisposable {
    private ILocalizationService localization;
    private IDalamudPluginInterface pluginInterface;

    public string TabName => this.localization.Translate("about_tab_name");
    public int SortOrder => 999;
    public bool IsSidePanelOpen => false;

    public AboutTab(ILocalizationService localization, IDalamudPluginInterface pluginInterface) {
        this.localization = localization;
        this.pluginInterface = pluginInterface;
    }

    public void Draw() {
        ImGui.TextWrapped(this.localization.Translate("about_description"));

        ImGui.Spacing();
        ImGui.Spacing();

        ImGui.TextDisabled(this.localization.Translate("about_features_title"));
        ImGui.Spacing();

        ImGui.BulletText(this.localization.Translate("about_feature_1"));
        ImGui.BulletText(this.localization.Translate("about_feature_2"));
        ImGui.BulletText(this.localization.Translate("about_feature_3"));
        ImGui.BulletText(this.localization.Translate("about_feature_4"));

        ImGui.Spacing();
        ImGui.Separator();
        ImGui.Spacing();

        var version = this.pluginInterface.Manifest.AssemblyVersion.ToString();
        ImGui.TextUnformatted(this.localization.Translate("about_version", version));

        ImGui.Spacing();

        ImGui.PushFont(UiBuilder.IconFont);
        var githubIcon = FontAwesomeIcon.CodeBranch.ToIconString();
        var discordIcon = FontAwesomeIcon.Comments.ToIconString();
        ImGui.PopFont();

        if (ImGui.Button($"{githubIcon} {this.localization.Translate("about_github")}")) Util.OpenLink("https://github.com/AlmerisBE/RolePlayer");

        ImGui.SameLine();

        if (ImGui.Button($"{discordIcon} {this.localization.Translate("about_discord")}")) Util.OpenLink("https://discord.gg/2GdUQdC3h9");
    }

    public void DrawSidePanel() { }

    public void Dispose() { }
}