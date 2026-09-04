namespace RolePlayer.UI.MainWindow.Tabs.SubTabs;

using Dalamud.Bindings.ImGui;
using Dalamud.Game.ClientState.Keys;
using RolePlayer.Core.Configuration.Contracts;
using RolePlayer.UI.Localization.Contracts;
using System;

public class GeneralConfigSubTab {
    private IConfigurationService configService;
    private ILocalizationService localization;

    public GeneralConfigSubTab(IConfigurationService configService, ILocalizationService localization) {
        this.configService = configService;
        this.localization = localization;
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

        if (changed) this.configService.Save();
    }
}