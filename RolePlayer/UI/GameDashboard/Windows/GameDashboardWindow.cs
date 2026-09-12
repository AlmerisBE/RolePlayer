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

    private DashboardControlComponent controlComponent;
    private DashboardVariablesComponent variablesComponent;
    private DashboardParticipantsComponent participantsComponent;
    private DashboardMessagesComponent messagesComponent;

    public GameDashboardWindow(
        IGameDashboardPresenter presenter,
        ILocalizationService localization,
        DashboardControlComponent controlComponent,
        DashboardVariablesComponent variablesComponent,
        DashboardParticipantsComponent participantsComponent,
        DashboardMessagesComponent messagesComponent)
        : base("Game Master Dashboard###RolePlayer_GameHost", ImGuiWindowFlags.NoScrollbar) {

        this.presenter = presenter;
        this.localization = localization;

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

        if (ImGui.BeginTable("GameHostLayout", 2, ImGuiTableFlags.BordersInnerV | ImGuiTableFlags.Resizable)) {
            ImGui.TableSetupColumn("Configuration", ImGuiTableColumnFlags.WidthStretch, 0.45f);
            ImGui.TableSetupColumn("Session", ImGuiTableColumnFlags.WidthStretch, 0.55f);
            ImGui.TableNextRow();

            // COLONNE GAUCHE
            ImGui.TableNextColumn();
            this.controlComponent.Draw();

            // COLONNE DROITE
            ImGui.TableNextColumn();

            ImGui.TextDisabled(this.localization.Translate("host_session_status"));
            ImGui.Spacing();

            if (isRunning) {
                ImGui.TextColored(new Vector4(0.2f, 0.8f, 0.2f, 1.0f), this.presenter.CurrentStageName);

                var timers = this.presenter.RemainingTimers;
                if (timers != null && timers.Count > 0) {
                    ImGui.Spacing();
                    foreach (var timer in timers) {
                        ImGui.TextColored(new Vector4(1f, 0.6f, 0f, 1f), $"Chronomètre : {Math.Floor(timer.TotalMinutes):00}:{timer.Seconds:00}");
                    }
                }

                ImGui.Spacing();
                ImGui.Separator();
                ImGui.Spacing();

                this.variablesComponent.Draw();
                this.participantsComponent.Draw();

                ImGui.Spacing();
                ImGui.Separator();
                ImGui.Spacing();

                this.messagesComponent.Draw();
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