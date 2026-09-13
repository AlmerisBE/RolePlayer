namespace RolePlayer.UI.GameDashboard.Windows;

using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Windowing;
using RolePlayer.Core.GameEngine.Models;
using RolePlayer.UI.GameDashboard.Components;
using RolePlayer.UI.GameDashboard.Contracts;
using RolePlayer.UI.Localization.Contracts;
using System;
using System.Numerics;

public class GameDashboardWindow : Window {
    private IGameDashboardPresenter presenter;
    private ILocalizationService localization;

    private DashboardHeaderComponent headerComponent;
    private DashboardFooterComponent footerComponent;
    private DashboardControlComponent controlComponent;
    private DashboardVariablesComponent variablesComponent;
    private DashboardParticipantsComponent participantsComponent;
    private DashboardMessagesComponent messagesComponent;

    public GameDashboardWindow(
        IGameDashboardPresenter presenter,
        ILocalizationService localization,
        DashboardHeaderComponent headerComponent,
        DashboardFooterComponent footerComponent,
        DashboardControlComponent controlComponent,
        DashboardVariablesComponent variablesComponent,
        DashboardParticipantsComponent participantsComponent,
        DashboardMessagesComponent messagesComponent)
        : base("Game Master Dashboard###RolePlayer_GameHost", ImGuiWindowFlags.NoScrollbar) {

        this.presenter = presenter;
        this.localization = localization;

        this.headerComponent = headerComponent;
        this.footerComponent = footerComponent;
        this.controlComponent = controlComponent;
        this.variablesComponent = variablesComponent;
        this.participantsComponent = participantsComponent;
        this.messagesComponent = messagesComponent;

        this.SizeConstraints = new WindowSizeConstraints {
            MinimumSize = new Vector2(700, 500),
            MaximumSize = new Vector2(float.MaxValue, float.MaxValue)
        };
    }

    public override void Draw() {
        var isRunning = this.presenter.CurrentState != SessionState.Inactive;

        // EN-TÊTE
        this.headerComponent.Draw();

        ImGui.Spacing();
        ImGui.Separator();
        ImGui.Spacing();

        float footerHeight = ImGui.GetFrameHeight() + ImGui.GetStyle().WindowPadding.Y + ImGui.GetStyle().ItemSpacing.Y * 2;

        if (ImGui.BeginChild("MainDashboardContent", new Vector2(0, -footerHeight), false, ImGuiWindowFlags.NoScrollbar)) {
            if (ImGui.BeginTable("GameHostLayout", 2, ImGuiTableFlags.BordersInnerV | ImGuiTableFlags.Resizable)) {
                ImGui.TableSetupColumn("Configuration", ImGuiTableColumnFlags.WidthStretch, 0.40f);
                ImGui.TableSetupColumn("Session", ImGuiTableColumnFlags.WidthStretch, 0.60f);
                ImGui.TableNextRow();

                // COLONNE GAUCHE (Progression)
                ImGui.TableNextColumn();
                this.controlComponent.Draw();

                // COLONNE DROITE (Détails)
                ImGui.TableNextColumn();

                if (ImGui.BeginChild("SessionDetailsChild", new Vector2(0, 0), false, ImGuiWindowFlags.None)) {
                    if (this.presenter.SelectedGame != null) {
                        ImGui.TextDisabled(this.localization.Translate("host_session_status"));
                        ImGui.Spacing();

                        if (isRunning) {
                            ImGui.TextColored(new Vector4(0.2f, 0.8f, 0.2f, 1.0f), this.presenter.CurrentStageName);

                            string desc = this.presenter.CurrentStageDescription;
                            if (!string.IsNullOrEmpty(desc)) {
                                ImGui.Spacing();
                                ImGui.PushStyleColor(ImGuiCol.Text, new Vector4(0.7f, 0.7f, 0.7f, 1.0f));
                                ImGui.TextWrapped(desc);
                                ImGui.PopStyleColor();
                            }

                            var timers = this.presenter.RemainingTimers;
                            if (timers != null && timers.Count > 0) {
                                ImGui.Spacing();
                                foreach (var timer in timers) {
                                    ImGui.TextColored(new Vector4(1f, 0.6f, 0f, 1f), $"Chronomètre : {Math.Floor(timer.TotalMinutes):00}:{timer.Seconds:00}");
                                }
                            }
                        }
                        else {
                            string stateString = this.localization.Translate($"host_state_{this.presenter.CurrentState.ToString().ToLowerInvariant()}");
                            ImGui.TextColored(new Vector4(0.8f, 0.8f, 0.8f, 1.0f), stateString);
                        }

                        ImGui.Spacing();
                        ImGui.Separator();
                        ImGui.Spacing();

                        this.variablesComponent.Draw();

                        if (isRunning) {
                            this.participantsComponent.Draw();
                            ImGui.Spacing();
                            ImGui.Separator();
                            ImGui.Spacing();
                        }

                        this.messagesComponent.Draw();
                    }
                    else {
                        ImGui.TextDisabled(this.localization.Translate("host_select_game"));
                    }
                }
                ImGui.EndChild();
                ImGui.EndTable();
            }
        }
        ImGui.EndChild();

        ImGui.Separator();
        ImGui.Spacing();

        this.footerComponent.Draw();
    }

    public void OpenForGame() {
        this.IsOpen = true;
        ImGui.SetWindowFocus();
    }
}