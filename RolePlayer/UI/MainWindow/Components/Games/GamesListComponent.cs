namespace RolePlayer.UI.MainWindow.Components.Games;

using Dalamud.Bindings.ImGui;
using Dalamud.Interface;
using Dalamud.Interface.Components;
using RolePlayer.Core.GameEngine.Contracts;
using RolePlayer.UI.GameDashboard.Contracts;
using RolePlayer.UI.GameDashboard.Windows;
using RolePlayer.UI.Localization.Contracts;
using RolePlayer.UI.MainWindow.Contracts;
using System.Linq;
using System.Numerics;

public class GamesListComponent {
    private IGameLibraryService libraryService;
    private ILocalizationService localization;
    private IGameDashboardPresenter dashboardPresenter;
    private GameDashboardWindow dashboardWindow;
    private IGameSerializerService serializerService;
    private IGameEditorWindow editorWindow;

    public GamesListComponent(
        IGameLibraryService libraryService,
        ILocalizationService localization,
        IGameDashboardPresenter dashboardPresenter,
        GameDashboardWindow dashboardWindow,
        IGameSerializerService serializerService,
        IGameEditorWindow editorWindow) {

        this.libraryService = libraryService;
        this.localization = localization;
        this.dashboardPresenter = dashboardPresenter;
        this.dashboardWindow = dashboardWindow;
        this.serializerService = serializerService;
        this.editorWindow = editorWindow;
    }

    public void Draw() {
        string syncText = this.localization.Translate("games_restore_defaults");
        string createText = this.localization.Translate("games_create_new");

        ImGui.PushFont(UiBuilder.IconFont);
        float syncIconWidth = ImGui.CalcTextSize(FontAwesomeIcon.Sync.ToIconString()).X;
        float createIconWidth = ImGui.CalcTextSize(FontAwesomeIcon.Plus.ToIconString()).X;
        ImGui.PopFont();

        float framePaddingX = ImGui.GetStyle().FramePadding.X * 2;
        float innerSpacingX = ImGui.GetStyle().ItemInnerSpacing.X;

        float syncBtnWidth = syncIconWidth + innerSpacingX + ImGui.CalcTextSize(syncText).X + framePaddingX;
        float createBtnWidth = createIconWidth + innerSpacingX + ImGui.CalcTextSize(createText).X + framePaddingX;

        float totalWidth = syncBtnWidth + createBtnWidth + ImGui.GetStyle().ItemSpacing.X;

        ImGui.SetCursorPosX(ImGui.GetWindowContentRegionMax().X - totalWidth);

        if (ImGuiComponents.IconButtonWithText(FontAwesomeIcon.Sync, syncText)) {
            this.libraryService.RestoreDefaultGames();
        }

        if (ImGui.IsItemHovered()) ImGui.SetTooltip(this.localization.Translate("games_restore_tooltip"));

        ImGui.SameLine();

        if (ImGuiComponents.IconButtonWithText(FontAwesomeIcon.Plus, createText)) {
            this.editorWindow.OpenForEditing(null);
        }

        ImGui.Spacing();
        ImGui.Separator();
        ImGui.Spacing();

        var games = this.libraryService.GetAvailableGames().ToList();
        if (games.Count == 0) return;

        if (ImGui.BeginTable("GamesListTable", 3, ImGuiTableFlags.Borders | ImGuiTableFlags.RowBg | ImGuiTableFlags.SizingFixedFit)) {
            ImGui.TableSetupColumn(this.localization.Translate("config_common_name"), ImGuiTableColumnFlags.WidthStretch);
            ImGui.TableSetupColumn("Author", ImGuiTableColumnFlags.WidthFixed, 120f);
            ImGui.TableSetupColumn(this.localization.Translate("config_common_actions"), ImGuiTableColumnFlags.WidthFixed, 110f);
            ImGui.TableHeadersRow();

            foreach (var game in games) {
                float rowHeight = 32f;
                ImGui.TableNextRow(ImGuiTableRowFlags.None, rowHeight);

                ImGui.TableNextColumn();
                ImGui.AlignTextToFramePadding();
                ImGui.Text(game.Name);

                if (ImGui.BeginPopupContextItem($"GameContextMenu_{game.Id}")) {
                    if (ImGui.MenuItem(this.localization.Translate("games_ctx_edit"))) this.editorWindow.OpenForEditing(game);
                    if (ImGui.MenuItem(this.localization.Translate("games_ctx_duplicate"))) this.libraryService.DuplicateGame(game.Id);
                    if (ImGui.MenuItem(this.localization.Translate("games_editor_export"))) {
                        var base64 = this.serializerService.ToBase64Export(game);
                        ImGui.SetClipboardText(base64);
                    }

                    ImGui.Separator();

                    ImGui.PushStyleColor(ImGuiCol.Text, new Vector4(0.8f, 0.2f, 0.2f, 1.0f));
                    if (ImGui.MenuItem(this.localization.Translate("games_ctx_delete"))) this.libraryService.DeleteGame(game.Id);
                    ImGui.PopStyleColor();

                    ImGui.EndPopup();
                }

                ImGui.TableNextColumn();
                ImGui.AlignTextToFramePadding();
                ImGui.TextDisabled(string.IsNullOrWhiteSpace(game.Author) ? "Unknown" : game.Author);

                ImGui.TableNextColumn();
                float buttonHeight = ImGui.GetFrameHeight();
                float startY = ImGui.GetCursorPosY() - ImGui.GetStyle().CellPadding.Y;
                ImGui.SetCursorPosY(startY + (rowHeight - buttonHeight) / 2f);

                ImGui.PushFont(UiBuilder.IconFont);
                ImGui.PushStyleColor(ImGuiCol.Button, new Vector4(0.2f, 0.4f, 0.8f, 1.0f));
                bool editClicked = ImGui.Button($"{FontAwesomeIcon.Edit.ToIconString()}##Edit_{game.Id}");
                ImGui.PopStyleColor();
                ImGui.PopFont();
                if (ImGui.IsItemHovered()) ImGui.SetTooltip(this.localization.Translate("games_ctx_edit"));

                ImGui.SameLine();

                ImGui.PushFont(UiBuilder.IconFont);
                ImGui.PushStyleColor(ImGuiCol.Button, new Vector4(0.2f, 0.8f, 0.2f, 1.0f));
                bool playClicked = ImGui.Button($"{FontAwesomeIcon.Play.ToIconString()}##Host_{game.Id}");
                ImGui.PopStyleColor();
                ImGui.PopFont();
                if (ImGui.IsItemHovered()) ImGui.SetTooltip(this.localization.Translate("games_host_session"));

                ImGui.SameLine();

                ImGui.PushFont(UiBuilder.IconFont);
                ImGui.PushStyleColor(ImGuiCol.Button, new Vector4(0.8f, 0.2f, 0.2f, 1.0f));
                bool deleteClicked = ImGui.Button($"{FontAwesomeIcon.Trash.ToIconString()}##Del_{game.Id}");
                ImGui.PopStyleColor();
                ImGui.PopFont();
                if (ImGui.IsItemHovered()) ImGui.SetTooltip(this.localization.Translate("games_ctx_delete"));

                if (editClicked) this.editorWindow.OpenForEditing(game);
                if (playClicked) {
                    this.dashboardPresenter.SelectGame(game);
                    this.dashboardWindow.OpenForGame();
                }
                if (deleteClicked) this.libraryService.DeleteGame(game.Id);
            }
            ImGui.EndTable();
        }
    }
}