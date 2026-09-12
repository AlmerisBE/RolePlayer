namespace RolePlayer.UI.GameDashboard.Components;

using Dalamud.Bindings.ImGui;
using RolePlayer.UI.GameDashboard.Contracts;
using System;
using System.Linq;
using System.Numerics;

public class DashboardVariablesComponent {
    private IGameDashboardPresenter presenter;

    public DashboardVariablesComponent(IGameDashboardPresenter presenter) {
        this.presenter = presenter;
    }

    public void Draw() {
        if (this.presenter.SelectedGame == null || this.presenter.SelectedGame.ExposedVariables.Count == 0) return;

        ImGui.TextDisabled("Paramètres du Jeu (Variables)");
        ImGui.Spacing();

        foreach (var varDef in this.presenter.SelectedGame.ExposedVariables) {
            string currentValRaw = this.presenter.SessionVariables.TryGetValue(varDef.Key, out var v) ? v?.ToString() ?? string.Empty : string.Empty;

            ImGui.Text(varDef.Label);
            ImGui.SetNextItemWidth(-1f);

            if (varDef.Type.Equals("Emote", StringComparison.OrdinalIgnoreCase)) {
                uint currentEmoteId = uint.TryParse(currentValRaw, out uint parsedId) ? parsedId : 0;
                var selectedEmote = this.presenter.EmotesCache.FirstOrDefault(e => e.Id == currentEmoteId);
                string preview = selectedEmote != null ? selectedEmote.Name : "Sélectionner une Emote...";

                if (ImGui.BeginCombo($"##var_combo_{varDef.Key}", preview)) {
                    foreach (var emote in this.presenter.EmotesCache.Where(e => e.IsUnlocked).OrderBy(e => e.Name)) {
                        if (ImGui.Selectable(emote.Name, emote.Id == currentEmoteId)) {
                            this.presenter.SetSessionVariable(varDef.Key, emote.Id);
                        }
                    }
                    ImGui.EndCombo();
                }
            }
            else if (varDef.Type.Equals("Number", StringComparison.OrdinalIgnoreCase)) {
                int currentInt = int.TryParse(currentValRaw, out int pInt) ? pInt : 0;
                if (ImGui.InputInt($"##var_num_{varDef.Key}", ref currentInt)) {
                    this.presenter.SetSessionVariable(varDef.Key, currentInt);
                }
            }
            else {
                if (ImGui.InputTextMultiline($"##var_str_{varDef.Key}", ref currentValRaw, 512, new Vector2(-1, ImGui.GetTextLineHeight() * 2))) {
                    this.presenter.SetSessionVariable(varDef.Key, currentValRaw);
                }
            }
            ImGui.Spacing();
        }

        ImGui.Spacing();
        ImGui.Separator();
        ImGui.Spacing();
    }
}