namespace RolePlayer.UI.GameDashboard.Windows;

using Dalamud.Bindings.ImGui;
using Dalamud.Interface;
using Dalamud.Interface.Windowing;
using RolePlayer.Core.GameEngine.Contracts;
using RolePlayer.Core.GameEngine.Models;
using RolePlayer.UI.GameDashboard.Components;
using RolePlayer.UI.Localization.Contracts;
using RolePlayer.UI.MainWindow.Contracts;
using System;
using System.Linq;
using System.Numerics;

public class GameEditorWindow : Window, IGameEditorWindow {
    private ILocalizationService localization;
    private IGameLibraryService libraryService;
    private IGameSerializerService serializerService;
    private GameStageEditorComponent stageEditor;

    // Gestion de l'état interne découplée
    private GameDefinition? selectedGame;
    private bool isCreatingNew;

    public GameEditorWindow(
        ILocalizationService localization,
        IGameLibraryService libraryService,
        IGameSerializerService serializerService,
        GameStageEditorComponent stageEditor)
        : base("Game Editor###RolePlayer_GameEditor", ImGuiWindowFlags.None) {

        this.localization = localization;
        this.libraryService = libraryService;
        this.serializerService = serializerService;
        this.stageEditor = stageEditor;

        this.SizeConstraints = new WindowSizeConstraints {
            MinimumSize = new Vector2(600, 700),
            MaximumSize = new Vector2(float.MaxValue, float.MaxValue)
        };
    }

    public void OpenForEditing(GameDefinition? game) {
        if (game == null) {
            this.isCreatingNew = true;
            this.selectedGame = new GameDefinition { Name = "New Game", Author = "Unknown" };
        }
        else {
            this.isCreatingNew = false;
            this.selectedGame = game;
        }

        this.IsOpen = true;
        ImGui.SetWindowFocus();
    }

    public override void OnClose() {
        this.selectedGame = null;
        this.isCreatingNew = false;
    }

    public override void PreDraw() {
        string title = this.isCreatingNew ? this.localization.Translate("games_create_new") : (this.selectedGame?.Name ?? "Game Editor");
        this.WindowName = $"{title}###RolePlayer_GameEditor";
    }

    public override void Draw() {
        var game = this.selectedGame;
        if (game == null) {
            this.IsOpen = false;
            return;
        }

        bool changed = false;

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

        this.stageEditor.Draw(game, ref changed);

        ImGui.Spacing();
        ImGui.Separator();
        ImGui.Spacing();

        ImGui.PushStyleColor(ImGuiCol.Button, new Vector4(0.2f, 0.6f, 0.2f, 1.0f));
        if (ImGui.Button(this.localization.Translate("games_editor_save"), new Vector2(-1, 35))) {
            this.libraryService.SaveGame(game);
            this.isCreatingNew = false;
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
                    importedGame.Id = Guid.NewGuid();
                    this.selectedGame = importedGame;
                    changed = true;
                }
            }
            catch { }
        }
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
                if (ImGui.Button($"{FontAwesomeIcon.Trash.ToIconString()}##DelVar_{i}")) keyToRemove = keys[i];
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