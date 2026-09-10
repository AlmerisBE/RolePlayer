namespace RolePlayer.UI.GameDashboard.Windows;

using Dalamud.Bindings.ImGui;
using Dalamud.Interface;
using Dalamud.Interface.Windowing;
using RolePlayer.Core.GameEngine.Models;
using RolePlayer.UI.GameDashboard.Contracts;
using RolePlayer.UI.Localization.Contracts;
using System;
using System.Numerics;

public class GameDashboardWindow : Window {
    private IGameDashboardPresenter presenter;
    private ILocalizationService localization;

    public GameDashboardWindow(IGameDashboardPresenter presenter, ILocalizationService localization)
        : base("Game Master Dashboard###RolePlayer_GameHost", ImGuiWindowFlags.NoScrollbar) {

        this.presenter = presenter;
        this.localization = localization;

        this.SizeConstraints = new WindowSizeConstraints {
            MinimumSize = new Vector2(500, 400),
            MaximumSize = new Vector2(float.MaxValue, float.MaxValue)
        };
    }

    public override void Draw() {
        var isRunning = this.presenter.CurrentState != SessionState.Inactive;

        if (ImGui.BeginTable("GameHostLayout", 2, ImGuiTableFlags.BordersInnerV | ImGuiTableFlags.Resizable)) {
            ImGui.TableSetupColumn("Configuration", ImGuiTableColumnFlags.WidthStretch, 0.4f);
            ImGui.TableSetupColumn("Session", ImGuiTableColumnFlags.WidthStretch, 0.6f);
            ImGui.TableNextRow();

            // Colonne de gauche : Configuration
            ImGui.TableNextColumn();

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

            foreach (GameChatChannel channel in Enum.GetValues(typeof(GameChatChannel))) {
                bool isSelected = this.presenter.SelectedChannels.Contains(channel);
                if (ImGui.Checkbox(channel.ToString(), ref isSelected)) this.presenter.ToggleChannel(channel);
            }

            ImGui.EndDisabled();

            // Colonne de droite : Statut de la Session
            ImGui.TableNextColumn();

            ImGui.TextDisabled(this.localization.Translate("host_session_status"));
            ImGui.Spacing();

            string stateString = this.localization.Translate($"host_state_{this.presenter.CurrentState.ToString().ToLowerInvariant()}");

            if (isRunning) ImGui.TextColored(new Vector4(0.2f, 0.8f, 0.2f, 1.0f), stateString);
            else ImGui.TextColored(new Vector4(0.8f, 0.8f, 0.8f, 1.0f), stateString);

            ImGui.Spacing();
            ImGui.Separator();
            ImGui.Spacing();

            if (!isRunning) {
                ImGui.BeginDisabled(this.presenter.SelectedGame == null || this.presenter.SelectedChannels.Count == 0);
                ImGui.PushStyleColor(ImGuiCol.Button, new Vector4(0.2f, 0.6f, 0.2f, 1.0f));

                ImGui.PushFont(UiBuilder.IconFont);
                string playIcon = FontAwesomeIcon.Play.ToIconString();
                ImGui.PopFont();

                if (ImGui.Button($"{playIcon} {this.localization.Translate("host_start_session")}", new Vector2(-1, 40))) this.presenter.StartSession();

                ImGui.PopStyleColor();
                ImGui.EndDisabled();
            }
            else {
                ImGui.PushStyleColor(ImGuiCol.Button, new Vector4(0.8f, 0.2f, 0.2f, 1.0f));

                ImGui.PushFont(UiBuilder.IconFont);
                string stopIcon = FontAwesomeIcon.Stop.ToIconString();
                ImGui.PopFont();

                if (ImGui.Button($"{stopIcon} {this.localization.Translate("host_stop_session")}", new Vector2(-1, 40))) this.presenter.StopSession();

                ImGui.PopStyleColor();
            }

            ImGui.EndTable();
        }
    }
}