namespace RolePlayer.UI.MainWindow.Tabs;

using Dalamud.Bindings.ImGui;
using Dalamud.Interface;
using Dalamud.Interface.Components;
using Dalamud.Plugin;
using RolePlayer.UI.EmoteBrowser.Contracts;
using RolePlayer.UI.Localization.Contracts;
using System;
using System.Diagnostics;

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

        ImGui.Bullet();
        ImGui.TextWrapped(this.localization.Translate("about_feature_1"));
        ImGui.Bullet();
        ImGui.TextWrapped(this.localization.Translate("about_feature_2"));
        ImGui.Bullet();
        ImGui.TextWrapped(this.localization.Translate("about_feature_3"));
        ImGui.Bullet();
        ImGui.TextWrapped(this.localization.Translate("about_feature_4"));

        ImGui.Spacing();
        ImGui.Separator();
        ImGui.Spacing();

        var version = this.pluginInterface.Manifest.AssemblyVersion.ToString();
        ImGui.TextUnformatted(this.localization.Translate("about_version", version));

        ImGui.Spacing();

        if (ImGuiComponents.IconButtonWithText(FontAwesomeIcon.CodeBranch, this.localization.Translate("about_github"))) {
            try {
                Process.Start(new ProcessStartInfo {
                    FileName = "https://github.com/AlmerisBE/RolePlayer",
                    UseShellExecute = true
                });
            }
            catch { } // Silently ignore if the OS fails to open the browser
        }

        ImGui.SameLine();

        if (ImGuiComponents.IconButtonWithText(FontAwesomeIcon.Comments, this.localization.Translate("about_discord"))) {
            try {
                Process.Start(new ProcessStartInfo {
                    FileName = "https://discord.gg/2GdUQdC3h9",
                    UseShellExecute = true
                });
            }
            catch { } // Silently ignore if the OS fails to open the browser
        }
    }

    public void DrawSidePanel() { }

    public void Dispose() { }
}