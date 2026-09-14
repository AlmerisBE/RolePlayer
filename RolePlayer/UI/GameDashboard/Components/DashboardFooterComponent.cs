namespace RolePlayer.UI.GameDashboard.Components;

using Dalamud.Bindings.ImGui;
using RolePlayer.Core.GameEngine.Models;
using RolePlayer.UI.GameDashboard.Contracts;
using RolePlayer.UI.Localization.Contracts;
using System.Numerics;

public class DashboardFooterComponent {
    private IGameDashboardPresenter presenter;
    private ILocalizationService localization;

    public DashboardFooterComponent(IGameDashboardPresenter presenter, ILocalizationService localization) {
        this.presenter = presenter;
        this.localization = localization;
    }

    public void Draw() {
        var isRunning = this.presenter.CurrentState != SessionState.Inactive;

        ImGui.AlignTextToFramePadding();
        ImGui.TextDisabled(this.localization.Translate("host_game_selection"));
        ImGui.SameLine();

        ImGui.BeginDisabled(isRunning);
        var selectedGameName = this.presenter.SelectedGame?.Name ?? this.localization.Translate("host_select_game");

        ImGui.SetNextItemWidth(200f);
        if (ImGui.BeginCombo("##GameSelection", selectedGameName)) {
            foreach (var game in this.presenter.AvailableGames) {
                if (ImGui.Selectable(game.Name, this.presenter.SelectedGame?.Id == game.Id)) this.presenter.SelectGame(game);
            }
            ImGui.EndCombo();
        }
        ImGui.EndDisabled();

        ImGui.SameLine();

        string joinText = this.localization.Translate("host_allow_registration");
        bool allowJoin = this.presenter.AllowChatRegistration;
        if (ImGui.Checkbox(joinText, ref allowJoin))
            this.presenter.AllowChatRegistration = allowJoin;

        float btnWidth = 130f;
        float resizeGripOffset = 15f;

        ImGui.SameLine(ImGui.GetWindowContentRegionMax().X - btnWidth - resizeGripOffset);

        if (!isRunning) {
            ImGui.BeginDisabled(this.presenter.SelectedGame == null || this.presenter.SelectedChannels.Count == 0);
            ImGui.PushStyleColor(ImGuiCol.Button, new Vector4(0.2f, 0.6f, 0.2f, 1.0f));
            if (ImGui.Button(this.localization.Translate("host_start_session"), new Vector2(btnWidth, 0))) this.presenter.StartSession();
            ImGui.PopStyleColor();
            ImGui.EndDisabled();
        }
        else {
            ImGui.PushStyleColor(ImGuiCol.Button, new Vector4(0.8f, 0.2f, 0.2f, 1.0f));
            if (ImGui.Button(this.localization.Translate("host_stop_session"), new Vector2(btnWidth, 0))) this.presenter.StopSession();
            ImGui.PopStyleColor();
        }
    }
}