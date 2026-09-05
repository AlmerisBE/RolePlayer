namespace RolePlayer.UI.MainWindow.Tabs.SubTabs;

using Dalamud.Bindings.ImGui;
using Dalamud.Game.ClientState.Keys;
using Dalamud.Interface;
using RolePlayer.Core.Configuration.Contracts;
using RolePlayer.UI.Localization.Contracts;
using RolePlayer.UI.Themes.Contracts;
using System;
using System.Linq;

public class GeneralConfigSubTab {
    private IConfigurationService configService;
    private ILocalizationService localization;
    private IThemeManagementService themeService;

    public GeneralConfigSubTab(IConfigurationService configService, ILocalizationService localization, IThemeManagementService themeService) {
        this.configService = configService;
        this.localization = localization;
        this.themeService = themeService;
    }

    public void Draw() {
        var config = this.configService.GetConfig();
        bool changed = false;

        ImGui.Text(this.localization.Translate("main_general_title"));
        ImGui.Separator();
        ImGui.Spacing();

        bool enableHotbars = config.EnableHotbars;
        if (ImGui.Checkbox($"{this.localization.Translate("main_general_enable_hotbars")}##enableHotbars", ref enableHotbars)) {
            config.EnableHotbars = enableHotbars;
            changed = true;
        }
        if (ImGui.IsItemHovered()) ImGui.SetTooltip(this.localization.Translate("main_general_enable_hotbars_tooltip"));

        ImGui.Spacing();
        ImGui.Separator();
        ImGui.Spacing();

        ImGui.Text(this.localization.Translate("main_general_hotkey_title"));
        ImGui.Spacing();

        bool ctrl = config.HotkeyCtrl;
        if (ImGui.Checkbox("Ctrl##hk", ref ctrl)) {
            config.HotkeyCtrl = ctrl;
            changed = true;
        }
        ImGui.SameLine();

        bool shift = config.HotkeyShift;
        if (ImGui.Checkbox("Shift##hk", ref shift)) {
            config.HotkeyShift = shift;
            changed = true;
        }
        ImGui.SameLine();

        bool alt = config.HotkeyAlt;
        if (ImGui.Checkbox("Alt##hk", ref alt)) {
            config.HotkeyAlt = alt;
            changed = true;
        }
        ImGui.SameLine();

        ImGui.SetNextItemWidth(150f);
        if (ImGui.BeginCombo("##hkKey", config.Hotkey.ToString())) {
            foreach (VirtualKey key in Enum.GetValues(typeof(VirtualKey))) {
                if (ImGui.Selectable(key.ToString(), config.Hotkey == key)) {
                    config.Hotkey = key;
                    changed = true;
                }
            }
            ImGui.EndCombo();
        }

        ImGui.Spacing();
        ImGui.Separator();
        ImGui.Spacing();

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

        string themePath = this.themeService.ThemeDirectory;
        ImGui.SetNextItemWidth(-1f);
        ImGui.InputText("##ThemeDirectoryPath", ref themePath, 512, ImGuiInputTextFlags.ReadOnly);

        ImGui.Spacing();
        ImGui.Spacing();

        var availableThemes = this.themeService.GetAvailableThemes().ToList();
        availableThemes.Insert(0, "Default");

        ImGui.Text(this.localization.Translate("config_themes_select"));
        ImGui.SetNextItemWidth(250f);

        if (ImGui.BeginCombo("##ThemeSelect", config.SelectedTheme)) {
            foreach (var theme in availableThemes) {
                if (ImGui.Selectable(theme, config.SelectedTheme == theme)) {
                    config.SelectedTheme = theme;
                    changed = true;
                    this.themeService.LoadTheme(theme);
                }
            }
            ImGui.EndCombo();
        }

        if (changed) this.configService.Save();
    }
}