namespace RolePlayer.UI.GameDashboard.Components;

using Dalamud.Bindings.ImGui;
using RolePlayer.Core.GameEngine.Models;
using RolePlayer.UI.GameDashboard.Contracts;
using RolePlayer.UI.Localization.Contracts;
using System;
using System.Numerics;

public class DashboardMessagesComponent {
    private IGameDashboardPresenter presenter;
    private ILocalizationService localization;

    public DashboardMessagesComponent(IGameDashboardPresenter presenter, ILocalizationService localization) {
        this.presenter = presenter;
        this.localization = localization;
    }

    public void Draw() {
        if (this.presenter.SelectedGame == null) return;
        var game = this.presenter.SelectedGame;

        if (ImGui.CollapsingHeader(this.localization.Translate("host_messages_title"))) {
            bool messagesChanged = false;

            ImGui.Spacing();
            ImGui.TextDisabled(this.localization.Translate("host_messages_desc"));
            ImGui.Spacing();

            foreach (var stage in game.Stages) {
                bool stageNodeOpen = ImGui.TreeNodeEx($"{this.localization.Translate("host_stage_prefix", stage.Name)}###MsgStage_{stage.Id}", ImGuiTreeNodeFlags.DefaultOpen);

                if (stageNodeOpen) {
                    for (int i = 0; i < stage.OnEnterActions.Count; i++) {
                        var action = stage.OnEnterActions[i];
                        if (action.ActionType.Equals("BroadcastMessage", StringComparison.OrdinalIgnoreCase)) {
                            messagesChanged |= this.DrawMessageInput($"##msg_enter_{stage.Id}_{i}", this.localization.Translate("host_enter_action_prefix", i + 1), action);
                        }
                    }

                    for (int m = 0; m < stage.ActiveModules.Count; m++) {
                        var module = stage.ActiveModules[m];
                        for (int a = 0; a < module.OnTriggerActions.Count; a++) {
                            var action = module.OnTriggerActions[a];
                            if (action.ActionType.Equals("BroadcastMessage", StringComparison.OrdinalIgnoreCase)) {
                                messagesChanged |= this.DrawMessageInput($"##msg_mod_{stage.Id}_{m}_{a}", this.localization.Translate("host_trigger_action_prefix", module.ModuleType, a + 1), action);
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

        float width = ImGui.GetContentRegionAvail().X;
        float height = ImGui.GetTextLineHeight() * 4;

        if (ImGui.InputTextMultiline(id, ref val, 512, new Vector2(width, height), ImGuiInputTextFlags.NoHorizontalScroll)) {
            action.Parameters["Message"] = val;
            changed = true;
        }

        ImGui.Spacing();
        return changed;
    }
}