namespace RolePlayer.UI.MainWindow.Tabs.SubTabs;

using Dalamud.Bindings.ImGui;
using Dalamud.Game.ClientState.Keys;
using RolePlayer.Core.Configuration.Contracts;
using System;

public class GeneralConfigSubTab {
    private IConfigurationService configService;

    public GeneralConfigSubTab(IConfigurationService configService) {
        this.configService = configService;
    }

    public void Draw() {
        var config = this.configService.GetConfig();
        bool changed = false;

        ImGui.Text("General Settings");
        ImGui.Separator();
        ImGui.Spacing();

        ImGui.Text("Global Hotkey to Toggle Main Window");
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

        if (changed) {
            this.configService.Save();
        }
    }
}