namespace RolePlayer.UI.GameDashboard.Components;

using Dalamud.Bindings.ImGui;
using Dalamud.Interface;
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

        ImGui.TextDisabled(this.localization.Translate("host_game_selection"));
        ImGui.Spacing();

        ImGui.BeginDisabled(isRunning);
        var selectedGameName = this.presenter.SelectedGame?.Name ?? this.localization.Translate("host_select_game");
        ImGui.SetNextItemWidth(-1f);
        if (ImGui.BeginCombo("##GameSelection", selectedGameName)) {
            foreach (var game in this.presenter.AvailableGames) {
                if (ImGui.Selectable(game.Name, this.presenter.SelectedGame?.Id == game.Id)) this.presenter.SelectGame(game);
            }
            ImGui.EndCombo();
        }

        ImGui.Spacing();
        ImGui.Separator();
        ImGui.Spacing();

        ImGui.TextDisabled(this.localization.Translate("host_listening_channels"));
        ImGui.Spacing();

        string channelPreview = this.presenter.SelectedChannels.Count == 0 ? this.localization.Translate("host_no_channel") : this.localization.Translate("host_channels_selected", this.presenter.SelectedChannels.Count);

        ImGui.SetNextItemWidth(-1f);
        if (ImGui.BeginCombo("##ListeningChannels", channelPreview)) {
            foreach (GameChatChannel channel in Enum.GetValues(typeof(GameChatChannel))) {
                bool isSelected = this.presenter.SelectedChannels.Contains(channel);
                if (ImGui.Checkbox(channel.ToString(), ref isSelected)) {
                    this.presenter.ToggleChannel(channel);
                }
            }
            ImGui.EndCombo();
        }
        ImGui.EndDisabled();

        ImGui.Spacing();
        ImGui.Separator();
        ImGui.Spacing();

        bool allowJoin = this.presenter.AllowChatRegistration;
        if (ImGui.Checkbox(this.localization.Translate("host_allow_registration"), ref allowJoin)) {
            this.presenter.AllowChatRegistration = allowJoin;
        }

        ImGui.Spacing();
        ImGui.Separator();
        ImGui.Spacing();

        if (!isRunning) {
            ImGui.BeginDisabled(this.presenter.SelectedGame == null || this.presenter.SelectedChannels.Count == 0);
            ImGui.PushStyleColor(ImGuiCol.Button, new Vector4(0.2f, 0.6f, 0.2f, 1.0f));

            string playIcon = FontAwesomeIcon.Play.ToIconString();
            if (ImGui.Button($"{playIcon} {this.localization.Translate("host_start_session")}", new Vector2(-1, 40))) this.presenter.StartSession();

            ImGui.PopStyleColor();
            ImGui.EndDisabled();
        }
        else {
            if (this.presenter.CurrentStageName != "Finished") {
                ImGui.PushStyleColor(ImGuiCol.Button, new Vector4(0.2f, 0.4f, 0.8f, 1.0f));
                string stepIcon = FontAwesomeIcon.StepForward.ToIconString();
                if (ImGui.Button($"{stepIcon} {this.localization.Translate("host_next_stage")}", new Vector2(-1, 35))) this.presenter.AdvanceStage();
                ImGui.PopStyleColor();
                ImGui.Spacing();
            }

            ImGui.PushStyleColor(ImGuiCol.Button, new Vector4(0.8f, 0.2f, 0.2f, 1.0f));
            string stopIcon = FontAwesomeIcon.Stop.ToIconString();
            if (ImGui.Button($"{stopIcon} {this.localization.Translate("host_stop_session")}", new Vector2(-1, 40))) this.presenter.StopSession();
            ImGui.PopStyleColor();
        }
    }
}