namespace RolePlayer.UI.GameDashboard.Windows;

using Dalamud.Bindings.ImGui;
using Dalamud.Interface;
using Dalamud.Interface.Windowing;
using RolePlayer.Core.GameEngine.Models;
using RolePlayer.UI.GameDashboard.Contracts;
using RolePlayer.UI.Localization.Contracts;
using System;
using System.Linq;
using System.Numerics;

public class GameDashboardWindow : Window {
    private IGameDashboardPresenter presenter;
    private ILocalizationService localization;

    private readonly string[] defaultMessageKeys = { "Msg_RegistrationOpened", "Msg_RegistrationClosed", "Msg_Join", "Msg_Welcome", "Msg_StartWarning", "Msg_Start", "Msg_FirstToRoll", "Msg_Loss", "Msg_RollNext" };

    public GameDashboardWindow(IGameDashboardPresenter presenter, ILocalizationService localization)
        : base("Game Master Dashboard###RolePlayer_GameHost", ImGuiWindowFlags.NoScrollbar) {

        this.presenter = presenter;
        this.localization = localization;

        this.SizeConstraints = new WindowSizeConstraints {
            MinimumSize = new Vector2(700, 500),
            MaximumSize = new Vector2(float.MaxValue, float.MaxValue)
        };
    }

    public override void Draw() {
        var isRunning = this.presenter.CurrentState != SessionState.Inactive;

        if (ImGui.BeginTable("GameHostLayout", 2, ImGuiTableFlags.BordersInnerV | ImGuiTableFlags.Resizable)) {
            ImGui.TableSetupColumn("Configuration", ImGuiTableColumnFlags.WidthStretch, 0.45f);
            ImGui.TableSetupColumn("Session", ImGuiTableColumnFlags.WidthStretch, 0.55f);
            ImGui.TableNextRow();

            // ==========================================
            // COLONNE GAUCHE : Gestion et Contrôles
            // ==========================================
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
                if (this.presenter.CurrentStageName != "Finished") {
                    ImGui.PushStyleColor(ImGuiCol.Button, new Vector4(0.2f, 0.4f, 0.8f, 1.0f));
                    if (ImGui.Button("Passer à l'étape suivante", new Vector2(-1, 35))) this.presenter.AdvanceStage();
                    ImGui.PopStyleColor();
                    ImGui.Spacing();
                }

                ImGui.PushStyleColor(ImGuiCol.Button, new Vector4(0.8f, 0.2f, 0.2f, 1.0f));
                ImGui.PushFont(UiBuilder.IconFont);
                string stopIcon = FontAwesomeIcon.Stop.ToIconString();
                ImGui.PopFont();

                if (ImGui.Button($"{stopIcon} {this.localization.Translate("host_stop_session")}", new Vector2(-1, 40))) this.presenter.StopSession();
                ImGui.PopStyleColor();
            }

            // ==========================================
            // COLONNE DROITE : Statut et Informations
            // ==========================================
            ImGui.TableNextColumn();

            ImGui.TextDisabled(this.localization.Translate("host_session_status"));
            ImGui.Spacing();

            if (isRunning) {
                ImGui.TextColored(new Vector4(0.2f, 0.8f, 0.2f, 1.0f), this.presenter.CurrentStageName);

                ImGui.Spacing();
                ImGui.Separator();
                ImGui.Spacing();

                if (this.presenter.CurrentStageName == "Preparation" && this.presenter.SelectedGame != null) {
                    ImGui.TextDisabled("Messages du jeu (Édition)");
                    if (ImGui.BeginChild("MessageEditorArea", new Vector2(0, 0), true)) {
                        bool messagesChanged = false;

                        foreach (var key in this.defaultMessageKeys) {
                            if (!this.presenter.SelectedGame.Messages.ContainsKey(key)) this.presenter.SelectedGame.Messages[key] = string.Empty;
                        }

                        foreach (var key in this.presenter.SelectedGame.Messages.Keys.ToList()) {
                            ImGui.TextDisabled(key);
                            string val = this.presenter.SelectedGame.Messages[key];
                            ImGui.SetNextItemWidth(-1f);
                            if (ImGui.InputText($"##msg_{key}", ref val, 256)) {
                                this.presenter.SelectedGame.Messages[key] = val;
                                messagesChanged = true;
                            }
                            ImGui.Spacing();
                        }

                        if (messagesChanged) this.presenter.SaveGameConfig();
                    }
                    ImGui.EndChild();
                }
                else if (this.presenter.CurrentStageName != "Preparation" && this.presenter.CurrentStageName != "Finished") {

                    if (this.presenter.CurrentStageName == "Registration") {
                        bool allowJoin = this.presenter.AllowChatRegistration;
                        if (ImGui.Checkbox("Autoriser inscriptions (!join)", ref allowJoin)) this.presenter.AllowChatRegistration = allowJoin;
                        ImGui.Spacing();
                    }

                    ImGui.TextDisabled(this.localization.Translate("host_participants"));
                    var players = this.presenter.Participants;

                    if (players.Count == 0) {
                        ImGui.TextDisabled(this.localization.Translate("host_no_participants"));
                    }
                    else {
                        string? participantToRemove = null;

                        // Vérification dynamique du paramètre global pour l'affichage des scores
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

                            // Récupération sécurisée du joueur actif
                            string currentPlayer = this.presenter.SessionVariables.TryGetValue("current_player", out var cp) ? cp?.ToString() ?? string.Empty : string.Empty;

                            for (int i = 0; i < players.Count; i++) {
                                ImGui.TableNextRow();

                                bool isCurrentTurn = string.Equals(players[i], currentPlayer, StringComparison.OrdinalIgnoreCase);

                                ImGui.TableNextColumn();
                                ImGui.AlignTextToFramePadding();
                                if (isCurrentTurn) {
                                    // Indicateur visuel du tour
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

                                // Affichage dynamique du score
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
            else {
                string stateString = this.localization.Translate($"host_state_{this.presenter.CurrentState.ToString().ToLowerInvariant()}");
                ImGui.TextColored(new Vector4(0.8f, 0.8f, 0.8f, 1.0f), stateString);
            }

            ImGui.EndTable();
        }
    }

    public void OpenForGame() {
        this.IsOpen = true;
        ImGui.SetWindowFocus();
    }
}