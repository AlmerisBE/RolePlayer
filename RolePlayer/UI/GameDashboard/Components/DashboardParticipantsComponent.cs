namespace RolePlayer.UI.GameDashboard.Components;

using Dalamud.Bindings.ImGui;
using Dalamud.Interface;
using RolePlayer.UI.GameDashboard.Contracts;
using RolePlayer.UI.Localization.Contracts;
using System;
using System.Numerics;

public class DashboardParticipantsComponent {
    private IGameDashboardPresenter presenter;
    private ILocalizationService localization;

    public DashboardParticipantsComponent(IGameDashboardPresenter presenter, ILocalizationService localization) {
        this.presenter = presenter;
        this.localization = localization;
    }

    public void Draw() {
        ImGui.TextDisabled(this.localization.Translate("host_participants"));
        var players = this.presenter.Participants;

        if (players.Count == 0) {
            ImGui.TextDisabled(this.localization.Translate("host_no_participants"));
        }
        else {
            string? participantToRemove = null;
            bool hasScores = this.presenter.SelectedGame != null &&
                             this.presenter.SelectedGame.Parameters.TryGetValue("TrackScores", out var ts) &&
                             ts.Equals("true", StringComparison.OrdinalIgnoreCase);

            int columnCount = hasScores ? 4 : 3;

            if (ImGui.BeginTable("ParticipantsTable", columnCount, ImGuiTableFlags.Borders | ImGuiTableFlags.RowBg | ImGuiTableFlags.ScrollY, new Vector2(0, 150))) {
                ImGui.TableSetupScrollFreeze(0, 1);
                ImGui.TableSetupColumn("#", ImGuiTableColumnFlags.WidthFixed, 30f);
                ImGui.TableSetupColumn(this.localization.Translate("config_common_name"), ImGuiTableColumnFlags.WidthStretch);
                if (hasScores) ImGui.TableSetupColumn("Score", ImGuiTableColumnFlags.WidthFixed, 60f);
                ImGui.TableSetupColumn(this.localization.Translate("config_common_actions"), ImGuiTableColumnFlags.WidthFixed, 30f);
                ImGui.TableHeadersRow();

                string currentPlayer = this.presenter.SessionVariables.TryGetValue("current_player", out var cp) ? cp?.ToString() ?? string.Empty : string.Empty;

                for (int i = 0; i < players.Count; i++) {
                    ImGui.TableNextRow();

                    bool isCurrentTurn = string.Equals(players[i], currentPlayer, StringComparison.OrdinalIgnoreCase);

                    ImGui.TableNextColumn();
                    ImGui.AlignTextToFramePadding();
                    if (isCurrentTurn) {
                        ImGui.PushFont(UiBuilder.IconFont);
                        ImGui.PushStyleColor(ImGuiCol.Text, new Vector4(0.2f, 0.8f, 0.2f, 1.0f));
                        ImGui.Text(FontAwesomeIcon.Play.ToIconString());
                        ImGui.PopStyleColor();
                        ImGui.PopFont();
                    }
                    else {
                        ImGui.Text((i + 1).ToString());
                    }

                    ImGui.TableNextColumn();
                    ImGui.AlignTextToFramePadding();
                    if (isCurrentTurn) ImGui.PushStyleColor(ImGuiCol.Text, new Vector4(0.2f, 0.8f, 0.2f, 1.0f));
                    ImGui.Text(players[i]);
                    if (isCurrentTurn) ImGui.PopStyleColor();

                    if (hasScores) {
                        ImGui.TableNextColumn();
                        ImGui.AlignTextToFramePadding();

                        string scoreKey = $"score_{players[i]}";
                        int score = 0;

                        if (this.presenter.SessionVariables.TryGetValue(scoreKey, out var s)) {
                            if (s is int sInt) score = sInt;
                            else if (s is string sStr && int.TryParse(sStr, out int parsedScore)) score = parsedScore;
                        }

                        ImGui.Text(score.ToString());
                    }

                    ImGui.TableNextColumn();
                    ImGui.PushStyleColor(ImGuiCol.Button, new Vector4(0.8f, 0.2f, 0.2f, 1.0f));
                    ImGui.PushFont(UiBuilder.IconFont);
                    if (ImGui.Button($"{FontAwesomeIcon.Trash.ToIconString()}##RemPart_{i}")) {
                        participantToRemove = players[i];
                    }
                    ImGui.PopFont();
                    ImGui.PopStyleColor();
                }
                ImGui.EndTable();
            }

            if (participantToRemove != null) {
                this.presenter.RemoveParticipant(participantToRemove);
            }
        }

        ImGui.Spacing();

        string targetName = this.presenter.CurrentTargetName;
        if (string.IsNullOrEmpty(targetName)) ImGui.BeginDisabled();
        if (ImGui.Button($"Ajouter Cible : {targetName ?? "Aucune"}", new Vector2(-1, 30))) this.presenter.AddTarget();
        if (string.IsNullOrEmpty(targetName)) ImGui.EndDisabled();
    }
}