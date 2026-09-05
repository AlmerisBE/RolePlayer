namespace RolePlayer.UI.MainWindow.Tabs.SubTabs;

using Dalamud.Bindings.ImGui;
using Dalamud.Interface;
using RolePlayer.Core.Configuration.Contracts;
using RolePlayer.UI.Localization.Contracts;
using RolePlayer.UI.Themes.Contracts;
using System.Linq;

public class ThemesConfigSubTab {
    private IConfigurationService configService;
    private IThemeManagementService themeService;
    private ILocalizationService localization;

    public ThemesConfigSubTab(IConfigurationService configService, IThemeManagementService themeService, ILocalizationService localization) {
        this.configService = configService;
        this.themeService = themeService;
        this.localization = localization;
    }

    public void Draw() {
        ImGui.Text(this.localization.Translate("config_themes_title"));
        ImGui.Separator();
        ImGui.Spacing();

        ImGui.TextWrapped(this.localization.Translate("config_themes_description"));
        ImGui.Spacing();

        ImGui.PushFont(UiBuilder.IconFont);
        if (ImGui.Button(FontAwesomeIcon.FolderOpen.ToIconString())) this.themeService.OpenThemeDirectory();
        ImGui.PopFont();

        ImGui.SameLine();
        ImGui.Text(this.localization.Translate("config_themes_open_folder"));

        ImGui.TextDisabled(this.themeService.ThemeDirectory);

        ImGui.Spacing();
        ImGui.Separator();
        ImGui.Spacing();

        var config = this.configService.GetConfig();
        var availableThemes = this.themeService.GetAvailableThemes().ToList();

        availableThemes.Insert(0, "Default");

        ImGui.Text(this.localization.Translate("config_themes_select"));
        ImGui.SetNextItemWidth(250f);

        if (ImGui.BeginCombo("##ThemeSelect", config.SelectedTheme)) {
            foreach (var theme in availableThemes) {
                if (ImGui.Selectable(theme, config.SelectedTheme == theme)) {
                    config.SelectedTheme = theme;
                    this.configService.Save();
                    this.themeService.LoadTheme(theme);
                }
            }
            ImGui.EndCombo();
        }
    }
}