namespace RolePlayer.UI.GameDashboard.Components;

using Dalamud.Bindings.ImGui;
using Dalamud.Interface;
using RolePlayer.Core.GameEngine.Contracts;
using RolePlayer.Core.GameEngine.Models;
using RolePlayer.UI.Localization.Contracts;
using RolePlayer.UI.MainWindow.Contracts;
using System;
using System.Linq;
using System.Numerics;

public class GameEditorPanelComponent {
    private IGameSelectionState selectionState;
    private ILocalizationService localization;
    private IGameLibraryService libraryService;
    private IGameSerializerService serializerService;

    public GameEditorPanelComponent(
        IGameSelectionState selectionState,
        ILocalizationService localization,
        IGameLibraryService libraryService,
        IGameSerializerService serializerService) {

        this.selectionState = selectionState;
        this.localization = localization;
        this.libraryService = libraryService;
        this.serializerService = serializerService;
    }

    public void Draw() {
        string closeIcon = FontAwesomeIcon.Times.ToIconString();
        ImGui.PushFont(UiBuilder.IconFont);
        var closeBtnWidth = ImGui.CalcTextSize(closeIcon).X + ImGui.GetStyle().FramePadding.X * 2;
        ImGui.PopFont();

        if (this.selectionState.IsCreatingNew && this.selectionState.SelectedGame == null) {
            this.selectionState.SelectedGame = new GameDefinition { Name = "New Game", Author = "Unknown" };
        }

        var game = this.selectionState.SelectedGame;
        if (game == null) return;

        bool changed = false;

        if (ImGui.BeginTable("GameEditorHeaderTable", 2)) {
            ImGui.TableSetupColumn("Title", ImGuiTableColumnFlags.WidthStretch);
            ImGui.TableSetupColumn("CloseBtn", ImGuiTableColumnFlags.WidthFixed, closeBtnWidth);

            ImGui.TableNextRow();
            ImGui.TableNextColumn();
            ImGui.AlignTextToFramePadding();
            ImGui.SetWindowFontScale(1.3f);

            string title = this.selectionState.IsCreatingNew ? this.localization.Translate("games_create_new") : game.Name;
            ImGui.TextUnformatted(title);
            ImGui.SetWindowFontScale(1.0f);

            ImGui.TableNextColumn();
            ImGui.PushFont(UiBuilder.IconFont);
            if (ImGui.Button($"{closeIcon}##CloseGameEditor")) {
                this.selectionState.SelectedGame = null;
                this.selectionState.IsCreatingNew = false;
                ImGui.PopFont();
                ImGui.EndTable();
                return;
            }
            ImGui.PopFont();

            ImGui.EndTable();
        }

        ImGui.Separator();
        ImGui.Spacing();

        if (ImGui.BeginChild("GameEditorScrollArea")) {
            ImGui.TextDisabled(this.localization.Translate("games_editor_general"));
            ImGui.Spacing();

            string name = game.Name;
            ImGui.SetNextItemWidth(-1f);
            if (ImGui.InputTextWithHint("##GameName", this.localization.Translate("games_editor_name"), ref name, 128)) {
                game.Name = name;
                changed = true;
            }

            string author = game.Author;
            ImGui.SetNextItemWidth(-1f);
            if (ImGui.InputTextWithHint("##GameAuthor", this.localization.Translate("games_editor_author"), ref author, 64)) {
                game.Author = author;
                changed = true;
            }

            string desc = game.Description;
            ImGui.SetNextItemWidth(-1f);
            if (ImGui.InputTextMultiline("##GameDesc", ref desc, 512, new Vector2(-1, ImGui.GetTextLineHeight() * 4))) {
                game.Description = desc;
                changed = true;
            }

            bool allowJoin = game.AllowChatRegistration;
            if (ImGui.Checkbox(this.localization.Translate("games_editor_allow_join"), ref allowJoin)) {
                game.AllowChatRegistration = allowJoin;
                changed = true;
            }

            ImGui.Spacing();
            ImGui.Separator();
            ImGui.Spacing();

            this.DrawVariablesEditor(game, ref changed);

            ImGui.Spacing();
            ImGui.Separator();
            ImGui.Spacing();

            // Placeholder pour l'éditeur d'étapes (Stages) que nous ferons à la prochaine itération
            ImGui.TextDisabled(this.localization.Translate("games_editor_stages"));
            ImGui.TextColored(new Vector4(0.6f, 0.6f, 0.6f, 1.0f), $"{game.Stages.Count} stage(s) configured.");

            ImGui.Spacing();
            ImGui.Separator();
            ImGui.Spacing();

            ImGui.PushStyleColor(ImGuiCol.Button, new Vector4(0.2f, 0.6f, 0.2f, 1.0f));
            if (ImGui.Button(this.localization.Translate("games_editor_save"), new Vector2(-1, 35))) {
                this.libraryService.SaveGame(game);
                this.selectionState.IsCreatingNew = false;
            }
            ImGui.PopStyleColor();

            ImGui.Spacing();

            if (ImGui.Button(this.localization.Translate("games_editor_export"), new Vector2(-1, 25))) {
                var base64 = this.serializerService.ToBase64Export(game);
                ImGui.SetClipboardText(base64);
            }

            if (ImGui.Button(this.localization.Translate("games_editor_import"), new Vector2(-1, 25))) {
                try {
                    var base64 = ImGui.GetClipboardText();
                    var importedGame = this.serializerService.FromBase64Import(base64);
                    if (importedGame != null) {
                        importedGame.Id = Guid.NewGuid(); // Ensure a unique ID to avoid overwriting existing games inadvertently
                        this.selectionState.SelectedGame = importedGame;
                        changed = true;
                    }
                }
                catch { }
            }
        }
        ImGui.EndChild();
    }

    private void DrawVariablesEditor(GameDefinition game, ref bool changed) {
        ImGui.TextDisabled(this.localization.Translate("games_editor_variables"));

        float availableWidth = ImGui.GetContentRegionAvail().X;
        float btnWidth = 32f;

        ImGui.SetCursorPosX(availableWidth - btnWidth);
        ImGui.PushFont(UiBuilder.IconFont);
        if (ImGui.Button($"{FontAwesomeIcon.Plus.ToIconString()}##AddVar", new Vector2(btnWidth, 0))) {
            game.InitialVariables[$"new_var_{game.InitialVariables.Count + 1}"] = 0;
            changed = true;
        }
        ImGui.PopFont();

        if (ImGui.IsItemHovered()) ImGui.SetTooltip(this.localization.Translate("games_editor_add_var"));

        if (game.InitialVariables.Count == 0) return;

        if (ImGui.BeginTable("VariablesTable", 3, ImGuiTableFlags.BordersInnerH | ImGuiTableFlags.SizingStretchProp)) {
            ImGui.TableSetupColumn(this.localization.Translate("games_editor_var_key"), ImGuiTableColumnFlags.WidthStretch, 0.45f);
            ImGui.TableSetupColumn(this.localization.Translate("games_editor_var_value"), ImGuiTableColumnFlags.WidthStretch, 0.45f);
            ImGui.TableSetupColumn("Act", ImGuiTableColumnFlags.WidthFixed, 30f);
            ImGui.TableHeadersRow();

            string? keyToRemove = null;
            var keys = game.InitialVariables.Keys.ToList();

            for (int i = 0; i < keys.Count; i++) {
                var key = keys[i];
                var valObj = game.InitialVariables[key];
                string valStr = valObj?.ToString() ?? string.Empty;

                ImGui.TableNextRow();
                ImGui.TableNextColumn();
                string newKey = key;
                ImGui.SetNextItemWidth(-1f);
                if (ImGui.InputText($"##VarKey_{i}", ref newKey, 64)) {
                    if (newKey != key && !string.IsNullOrWhiteSpace(newKey) && !game.InitialVariables.ContainsKey(newKey)) {
                        game.InitialVariables.Remove(key);
                        game.InitialVariables[newKey] = valObj ?? string.Empty;
                        changed = true;
                    }
                }

                ImGui.TableNextColumn();
                ImGui.SetNextItemWidth(-1f);
                if (ImGui.InputText($"##VarVal_{i}", ref valStr, 64)) {
                    if (int.TryParse(valStr, out int intVal)) game.InitialVariables[keys[i]] = intVal;
                    else game.InitialVariables[keys[i]] = valStr;
                    changed = true;
                }

                ImGui.TableNextColumn();
                ImGui.PushFont(UiBuilder.IconFont);
                ImGui.PushStyleColor(ImGuiCol.Button, new Vector4(0.8f, 0.2f, 0.2f, 1.0f));
                if (ImGui.Button($"{FontAwesomeIcon.Trash.ToIconString()}##DelVar_{i}")) {
                    keyToRemove = keys[i];
                }
                ImGui.PopStyleColor();
                ImGui.PopFont();
            }
            ImGui.EndTable();

            if (keyToRemove != null) {
                game.InitialVariables.Remove(keyToRemove);
                changed = true;
            }
        }
    }
}