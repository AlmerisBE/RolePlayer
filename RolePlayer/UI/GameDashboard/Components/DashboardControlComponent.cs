namespace RolePlayer.UI.GameDashboard.Components;

using Dalamud.Bindings.ImGui;
using RolePlayer.Core.GameEngine.Models;
using RolePlayer.UI.GameDashboard.Contracts;
using RolePlayer.UI.Localization.Contracts;
using System;
using System.Numerics;

public class DashboardControlComponent {
    private IGameDashboardPresenter presenter;
    private ILocalizationService localization;

    public DashboardControlComponent(IGameDashboardPresenter presenter, ILocalizationService localization) {
        this.presenter = presenter;
        this.localization = localization;
    }

    public void Draw() {
        var isRunning = this.presenter.CurrentState != SessionState.Inactive;

        if (this.presenter.SelectedGame != null && this.presenter.SelectedGame.Stages.Count > 0) {
            ImGui.TextDisabled(this.localization.Translate("host_stages_list"));
            ImGui.Spacing();

            float bottomSpace = 0f;
            if (isRunning && this.presenter.CurrentState != SessionState.Finished) {
                bottomSpace += 35f + ImGui.GetStyle().ItemSpacing.Y * 2;
            }
            if (isRunning && !string.IsNullOrEmpty(this.presenter.LastErrorKey)) {
                bottomSpace += ImGui.GetTextLineHeight() * 3;
            }

            if (ImGui.BeginChild("StagesListDisplay", new Vector2(-1, -bottomSpace), true, ImGuiWindowFlags.None)) {
                foreach (var stage in this.presenter.SelectedGame.Stages) {
                    bool isCurrent = isRunning && this.presenter.CurrentStageName.Equals(stage.Name, StringComparison.OrdinalIgnoreCase);

                    if (isCurrent) {
                        ImGui.PushStyleColor(ImGuiCol.Text, new Vector4(0.2f, 0.8f, 0.2f, 1.0f));
                        ImGui.Text($"▶ {stage.Name}");
                        ImGui.PopStyleColor();
                    }
                    else {
                        ImGui.TextDisabled($"   {stage.Name}");
                    }
                }
            }
            ImGui.EndChild();
        }

        ImGui.Spacing();

        if (isRunning) {
            if (!string.IsNullOrEmpty(this.presenter.LastErrorKey)) {
                ImGui.PushStyleColor(ImGuiCol.Text, new Vector4(0.9f, 0.3f, 0.3f, 1.0f));
                ImGui.TextWrapped(this.localization.Translate(this.presenter.LastErrorKey));
                ImGui.PopStyleColor();
                ImGui.Spacing();
            }

            if (this.presenter.CurrentState != SessionState.Finished) {
                string nextStageName = this.presenter.NextManualStageName;
                string btnText = string.IsNullOrEmpty(nextStageName)
                    ? this.localization.Translate("host_next_stage")
                    : this.localization.Translate("host_next_stage_specific", nextStageName);

                ImGui.PushStyleColor(ImGuiCol.Button, new Vector4(0.2f, 0.4f, 0.8f, 1.0f));
                if (ImGui.Button(btnText, new Vector2(-1, 35))) this.presenter.AdvanceStage();
                ImGui.PopStyleColor();
            }
        }
    }
}