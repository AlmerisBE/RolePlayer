namespace RolePlayer.UI.GameDashboard.Components;

using Dalamud.Bindings.ImGui;
using RolePlayer.Core.GameEngine.Models;
using RolePlayer.UI.GameDashboard.Contracts;
using System;
using System.Numerics;

public class DashboardMessagesComponent {
    private IGameDashboardPresenter presenter;

    public DashboardMessagesComponent(IGameDashboardPresenter presenter) {
        this.presenter = presenter;
    }

    public void Draw() {
        if (this.presenter.SelectedGame == null) return;
        var game = this.presenter.SelectedGame;

        if (ImGui.CollapsingHeader("Messages du jeu (Édition)")) {
            bool messagesChanged = false;

            ImGui.Spacing();
            ImGui.TextDisabled("Modifiez ici les textes des actions BroadcastMessage de chaque étape.");
            ImGui.Spacing();

            foreach (var stage in game.Stages) {
                bool stageNodeOpen = ImGui.TreeNodeEx($"Étape : {stage.Name}###MsgStage_{stage.Id}", ImGuiTreeNodeFlags.DefaultOpen);

                if (stageNodeOpen) {
                    // Actions à l'entrée de l'étape
                    for (int i = 0; i < stage.OnEnterActions.Count; i++) {
                        var action = stage.OnEnterActions[i];
                        if (action.ActionType.Equals("BroadcastMessage", StringComparison.OrdinalIgnoreCase)) {
                            messagesChanged |= this.DrawMessageInput($"##msg_enter_{stage.Id}_{i}", $"Action d'entrée #{i + 1}", action);
                        }
                    }

                    // Actions déclenchées par les modules (Chat, Timer, Emote, etc.)
                    for (int m = 0; m < stage.ActiveModules.Count; m++) {
                        var module = stage.ActiveModules[m];
                        for (int a = 0; a < module.OnTriggerActions.Count; a++) {
                            var action = module.OnTriggerActions[a];
                            if (action.ActionType.Equals("BroadcastMessage", StringComparison.OrdinalIgnoreCase)) {
                                messagesChanged |= this.DrawMessageInput($"##msg_mod_{stage.Id}_{m}_{a}", $"Déclencheur [{module.ModuleType}] - Action #{a + 1}", action);
                            }
                        }
                    }
                    ImGui.TreePop();
                }
            }

            if (messagesChanged) this.presenter.SaveGameConfig();
        }
    }

    private bool DrawMessageInput(string id, string label, GameActionConfig action) {
        bool changed = false;
        ImGui.TextDisabled(label);

        string val = action.Parameters.TryGetValue("Message", out var msg) ? msg : string.Empty;

        ImGui.SetNextItemWidth(-1f);
        if (ImGui.InputTextMultiline(id, ref val, 512, new Vector2(-1, ImGui.GetTextLineHeight() * 3))) {
            action.Parameters["Message"] = val;
            changed = true;
        }

        ImGui.Spacing();
        return changed;
    }
}