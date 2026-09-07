namespace RolePlayer.UI.MainWindow.Components.Macros;

using Dalamud.Bindings.ImGui;
using Dalamud.Interface;
using RolePlayer.Core.Macros.Models;
using RolePlayer.UI.Hotbar.Contracts;
using RolePlayer.UI.Localization.Contracts;
using RolePlayer.UI.MainWindow.Contracts;
using System.Numerics;

public class MacroCreationComponent {
    private IMacroManagementService macroService;
    private ILocalizationService localization;
    private IMacroSelectionState selectionState;
    private string newMacroName = string.Empty;

    public MacroCreationComponent(IMacroManagementService macroService, ILocalizationService localization, IMacroSelectionState selectionState) {
        this.macroService = macroService;
        this.localization = localization;
        this.selectionState = selectionState;
    }

    public void Draw() {
        ImGui.Text(this.localization.Translate("config_macro_create"));

        float availableWidth = ImGui.GetContentRegionAvail().X;
        float buttonWidth = 32f;
        float spacing = ImGui.GetStyle().ItemSpacing.X;

        ImGui.SetNextItemWidth(availableWidth - buttonWidth - spacing);
        ImGui.InputTextWithHint("##NewMacroName", this.localization.Translate("config_macro_name_hint"), ref this.newMacroName, 64);
        ImGui.SameLine();

        ImGui.PushFont(UiBuilder.IconFont);
        if (ImGui.Button($"{FontAwesomeIcon.Plus.ToIconString()}##AddMacro", new Vector2(buttonWidth, 0)) && !string.IsNullOrWhiteSpace(this.newMacroName)) {
            var newMacro = new RoleplayMacro {
                Name = this.newMacroName.Trim(),
                IconId = 66001
            };
            this.macroService.CreateMacro(newMacro);
            this.selectionState.SelectedMacro = newMacro;
            this.newMacroName = string.Empty;
        }
        ImGui.PopFont();
    }
}