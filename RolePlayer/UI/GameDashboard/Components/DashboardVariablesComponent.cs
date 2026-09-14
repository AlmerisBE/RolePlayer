namespace RolePlayer.UI.GameDashboard.Components;

using Dalamud.Bindings.ImGui;
using RolePlayer.UI.GameDashboard.Contracts;
using RolePlayer.UI.Localization.Contracts;
using System;
using System.Linq;

public class DashboardVariablesComponent {
    private IGameDashboardPresenter presenter;
    private ILocalizationService localization;

    public DashboardVariablesComponent(IGameDashboardPresenter presenter, ILocalizationService localization) {
        this.presenter = presenter;
        this.localization = localization;
    }

    public void Draw() {
        if (this.presenter.SelectedGame == null || this.presenter.SelectedGame.ExposedVariables.Count == 0) return;

        ImGui.TextDisabled(this.localization.Translate("host_variables_title"));
        ImGui.Spacing();

        foreach (var varDef in this.presenter.SelectedGame.ExposedVariables) {
            string currentValRaw = this.presenter.ActiveVariables.TryGetValue(varDef.Key, out var v) ? v?.ToString() ?? string.Empty : string.Empty;

            ImGui.Text(varDef.Label);

            if (varDef.Type.Equals("Emote", StringComparison.OrdinalIgnoreCase)) {
                uint currentEmoteId = uint.TryParse(currentValRaw, out uint parsedId) ? parsedId : 0;
                var selectedEmote = this.presenter.EmotesCache.FirstOrDefault(e => e.Id == currentEmoteId);
                string preview = selectedEmote != null ? selectedEmote.Name : this.localization.Translate("host_variables_select_emote");

                ImGui.SetNextItemWidth(-1f);
                if (ImGui.BeginCombo($"##var_combo_{varDef.Key}", preview)) {
                    foreach (var emote in this.presenter.EmotesCache.Where(e => e.IsUnlocked).OrderBy(e => e.Name)) {
                        if (ImGui.Selectable(emote.Name, emote.Id == currentEmoteId)) {
                            this.presenter.SetVariable(varDef.Key, emote.Id);
                        }
                    }
                    ImGui.EndCombo();
                }
            }
            else if (varDef.Type.Equals("Number", StringComparison.OrdinalIgnoreCase)) {
                int currentInt = int.TryParse(currentValRaw, out int pInt) ? pInt : 0;

                ImGui.SetNextItemWidth(-1f);
                if (ImGui.InputInt($"##var_num_{varDef.Key}", ref currentInt)) {
                    this.presenter.SetVariable(varDef.Key, currentInt);
                }
            }
            else {
                ImGui.SetNextItemWidth(-1f);
                if (ImGui.InputText($"##var_str_{varDef.Key}", ref currentValRaw, 512)) {
                    this.presenter.SetVariable(varDef.Key, currentValRaw);
                }
            }
            ImGui.Spacing();
        }

        ImGui.Spacing();
        ImGui.Separator();
        ImGui.Spacing();
    }
}