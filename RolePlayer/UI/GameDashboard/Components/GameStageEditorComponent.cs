namespace RolePlayer.UI.GameDashboard.Components;

using Dalamud.Bindings.ImGui;
using Dalamud.Interface;
using RolePlayer.Core.GameEngine.Models;
using RolePlayer.UI.Localization.Contracts;
using System.Linq;
using System.Numerics;

public class GameStageEditorComponent {
    private ILocalizationService localization;

    private readonly string[] moduleTypes = { "ChatListener", "DiceListener", "EmoteListener" };
    private readonly string[] actionTypes = { "RegisterPlayer", "SetVariable", "BroadcastMessage", "AdvanceStage", "StopGame" };
    private readonly string[] triggerTypes = { "Manual", "Auto", "OnEvent" };

    public GameStageEditorComponent(ILocalizationService localization) {
        this.localization = localization;
    }

    public void Draw(GameDefinition game, ref bool changed) {
        ImGui.TextDisabled(this.localization.Translate("games_editor_stages"));
        ImGui.SameLine(ImGui.GetContentRegionAvail().X - 120f);

        ImGui.PushFont(UiBuilder.IconFont);
        if (ImGui.Button($"{FontAwesomeIcon.Plus.ToIconString()} {this.localization.Translate("games_editor_add_stage")}", new Vector2(120f, 0))) {
            game.Stages.Add(new GameStage { Id = $"stage_{game.Stages.Count + 1}", Name = "New Stage" });
            changed = true;
        }
        ImGui.PopFont();

        ImGui.Spacing();

        GameStage? stageToRemove = null;

        for (int i = 0; i < game.Stages.Count; i++) {
            var stage = game.Stages[i];

            ImGui.PushID($"Stage_{i}");
            if (ImGui.CollapsingHeader($"{stage.Name} ({stage.Id})###Header_{i}")) {
                ImGui.Indent();
                ImGui.Spacing();

                this.DrawStageCoreFields(stage, ref changed);

                ImGui.Spacing();
                ImGui.Separator();
                ImGui.Spacing();

                this.DrawModulesEditor(stage, ref changed);

                ImGui.Spacing();
                ImGui.Separator();
                ImGui.Spacing();

                this.DrawTransitionsEditor(stage, ref changed);

                ImGui.Spacing();
                ImGui.PushStyleColor(ImGuiCol.Button, new Vector4(0.8f, 0.2f, 0.2f, 1.0f));
                if (ImGui.Button($"{FontAwesomeIcon.Trash.ToIconString()} Delete Stage")) stageToRemove = stage;
                ImGui.PopStyleColor();

                ImGui.Unindent();
                ImGui.Spacing();
            }
            ImGui.PopID();
        }

        if (stageToRemove != null) {
            game.Stages.Remove(stageToRemove);
            changed = true;
        }
    }

    private void DrawStageCoreFields(GameStage stage, ref bool changed) {
        string id = stage.Id;
        ImGui.SetNextItemWidth(150f);
        if (ImGui.InputTextWithHint("##StageId", this.localization.Translate("games_editor_stage_id"), ref id, 64)) {
            stage.Id = id.Replace(" ", "_").ToLowerInvariant();
            changed = true;
        }

        ImGui.SameLine();
        string name = stage.Name;
        ImGui.SetNextItemWidth(-1f);
        if (ImGui.InputTextWithHint("##StageName", this.localization.Translate("games_editor_stage_name"), ref name, 128)) {
            stage.Name = name;
            changed = true;
        }

        string desc = stage.GmDescription;
        ImGui.SetNextItemWidth(-1f);
        if (ImGui.InputTextMultiline("##StageDesc", ref desc, 512, new Vector2(-1, ImGui.GetTextLineHeight() * 3))) {
            stage.GmDescription = desc;
            changed = true;
        }
        if (ImGui.IsItemHovered()) ImGui.SetTooltip(this.localization.Translate("games_editor_stage_desc"));
    }

    private void DrawModulesEditor(GameStage stage, ref bool changed) {
        ImGui.TextDisabled(this.localization.Translate("games_editor_modules"));
        ImGui.SameLine(ImGui.GetContentRegionAvail().X - 120f);
        if (ImGui.Button(this.localization.Translate("games_editor_add_module"), new Vector2(120f, 0))) {
            stage.ActiveModules.Add(new GameModuleConfig { ModuleType = "ChatListener" });
            changed = true;
        }

        GameModuleConfig? moduleToRemove = null;

        for (int i = 0; i < stage.ActiveModules.Count; i++) {
            var module = stage.ActiveModules[i];
            ImGui.PushID($"Module_{i}");

            if (ImGui.BeginChild($"ModChild_{i}", new Vector2(-1, 0), true, ImGuiWindowFlags.AlwaysAutoResize)) {
                ImGui.SetNextItemWidth(150f);
                if (ImGui.BeginCombo("##ModuleType", module.ModuleType)) {
                    foreach (var type in this.moduleTypes) {
                        if (ImGui.Selectable(type, module.ModuleType == type)) {
                            module.ModuleType = type;
                            changed = true;
                        }
                    }
                    ImGui.EndCombo();
                }

                ImGui.SameLine(ImGui.GetContentRegionAvail().X - 30f);
                ImGui.PushStyleColor(ImGuiCol.Button, new Vector4(0.8f, 0.2f, 0.2f, 1.0f));
                if (ImGui.Button(FontAwesomeIcon.Trash.ToIconString())) moduleToRemove = module;
                ImGui.PopStyleColor();

                this.DrawDictionaryEditor("Parameters", module.Parameters, ref changed);
                this.DrawStringListEditor(this.localization.Translate("games_editor_conditions"), module.ConditionExpressions, ref changed);
                this.DrawActionsEditor(module, ref changed);
            }
            ImGui.EndChild();
            ImGui.PopID();
        }

        if (moduleToRemove != null) {
            stage.ActiveModules.Remove(moduleToRemove);
            changed = true;
        }
    }

    private void DrawActionsEditor(GameModuleConfig module, ref bool changed) {
        ImGui.TextDisabled(this.localization.Translate("games_editor_actions"));
        ImGui.SameLine(ImGui.GetContentRegionAvail().X - 100f);
        if (ImGui.Button(this.localization.Translate("games_editor_add_action"), new Vector2(100f, 0))) {
            module.OnTriggerActions.Add(new GameActionConfig { ActionType = "BroadcastMessage" });
            changed = true;
        }

        GameActionConfig? actionToRemove = null;

        for (int i = 0; i < module.OnTriggerActions.Count; i++) {
            var action = module.OnTriggerActions[i];
            ImGui.PushID($"Action_{i}");

            ImGui.SetNextItemWidth(150f);
            if (ImGui.BeginCombo("##ActionType", action.ActionType)) {
                foreach (var type in this.actionTypes) {
                    if (ImGui.Selectable(type, action.ActionType == type)) {
                        action.ActionType = type;
                        changed = true;
                    }
                }
                ImGui.EndCombo();
            }

            ImGui.SameLine();
            this.DrawDictionaryEditor($"Params##Act_{i}", action.Parameters, ref changed, true);

            ImGui.SameLine();
            ImGui.PushStyleColor(ImGuiCol.Button, new Vector4(0.8f, 0.2f, 0.2f, 1.0f));
            if (ImGui.Button(FontAwesomeIcon.Trash.ToIconString())) actionToRemove = action;
            ImGui.PopStyleColor();

            ImGui.PopID();
        }

        if (actionToRemove != null) {
            module.OnTriggerActions.Remove(actionToRemove);
            changed = true;
        }
    }

    private void DrawTransitionsEditor(GameStage stage, ref bool changed) {
        ImGui.TextDisabled(this.localization.Translate("games_editor_transitions"));
        ImGui.SameLine(ImGui.GetContentRegionAvail().X - 120f);
        if (ImGui.Button(this.localization.Translate("games_editor_add_transition"), new Vector2(120f, 0))) {
            stage.Transitions.Add(new GameTransition());
            changed = true;
        }

        GameTransition? transitionToRemove = null;

        for (int i = 0; i < stage.Transitions.Count; i++) {
            var transition = stage.Transitions[i];
            ImGui.PushID($"Trans_{i}");

            ImGui.SetNextItemWidth(100f);
            if (ImGui.BeginCombo("##TriggerType", transition.TriggerType)) {
                foreach (var type in this.triggerTypes) {
                    if (ImGui.Selectable(type, transition.TriggerType == type)) {
                        transition.TriggerType = type;
                        changed = true;
                    }
                }
                ImGui.EndCombo();
            }

            ImGui.SameLine();
            string target = transition.TargetStageId;
            ImGui.SetNextItemWidth(120f);
            if (ImGui.InputTextWithHint("##Target", this.localization.Translate("games_editor_transition_target"), ref target, 64)) {
                transition.TargetStageId = target;
                changed = true;
            }

            ImGui.SameLine();
            string cond = transition.ConditionExpression;
            ImGui.SetNextItemWidth(180f);
            if (ImGui.InputTextWithHint("##Cond", "Condition", ref cond, 128)) {
                transition.ConditionExpression = cond;
                changed = true;
            }

            ImGui.SameLine();
            ImGui.PushStyleColor(ImGuiCol.Button, new Vector4(0.8f, 0.2f, 0.2f, 1.0f));
            if (ImGui.Button(FontAwesomeIcon.Trash.ToIconString())) transitionToRemove = transition;
            ImGui.PopStyleColor();

            ImGui.PopID();
        }

        if (transitionToRemove != null) {
            stage.Transitions.Remove(transitionToRemove);
            changed = true;
        }
    }

    private void DrawStringListEditor(string title, System.Collections.Generic.List<string> list, ref bool changed) {
        ImGui.TextDisabled(title);
        ImGui.SameLine(ImGui.GetContentRegionAvail().X - 30f);
        if (ImGui.Button($"+##add_{title}")) {
            list.Add("");
            changed = true;
        }

        int indexToRemove = -1;
        for (int i = 0; i < list.Count; i++) {
            string val = list[i];
            ImGui.SetNextItemWidth(-40f);
            if (ImGui.InputText($"##str_{title}_{i}", ref val, 128)) {
                list[i] = val;
                changed = true;
            }
            ImGui.SameLine();
            ImGui.PushStyleColor(ImGuiCol.Button, new Vector4(0.8f, 0.2f, 0.2f, 1.0f));
            if (ImGui.Button($"-##del_{title}_{i}")) indexToRemove = i;
            ImGui.PopStyleColor();
        }

        if (indexToRemove >= 0) {
            list.RemoveAt(indexToRemove);
            changed = true;
        }
    }

    private void DrawDictionaryEditor(string title, System.Collections.Generic.Dictionary<string, string> dict, ref bool changed, bool compact = false) {
        if (!compact) ImGui.TextDisabled(title);

        if (ImGui.Button($"+##add_{title}")) {
            dict[$"key_{dict.Count}"] = "";
            changed = true;
        }

        string? keyToRemove = null;
        var keys = dict.Keys.ToList();

        for (int i = 0; i < keys.Count; i++) {
            var key = keys[i];
            var val = dict[key];

            if (!compact) ImGui.Dummy(new Vector2(10f, 0));
            ImGui.SameLine();

            string newKey = key;
            ImGui.SetNextItemWidth(100f);
            if (ImGui.InputText($"##k_{title}_{i}", ref newKey, 64)) {
                if (newKey != key && !string.IsNullOrWhiteSpace(newKey) && !dict.ContainsKey(newKey)) {
                    dict.Remove(key);
                    dict[newKey] = val;
                    changed = true;
                }
            }

            ImGui.SameLine();
            ImGui.SetNextItemWidth(compact ? 150f : -40f);
            if (ImGui.InputText($"##v_{title}_{i}", ref val, 256)) {
                dict[keys[i]] = val;
                changed = true;
            }

            ImGui.SameLine();
            ImGui.PushStyleColor(ImGuiCol.Button, new Vector4(0.8f, 0.2f, 0.2f, 1.0f));
            if (ImGui.Button($"-##del_{title}_{i}")) keyToRemove = keys[i];
            ImGui.PopStyleColor();
        }

        if (keyToRemove != null) {
            dict.Remove(keyToRemove);
            changed = true;
        }
    }
}