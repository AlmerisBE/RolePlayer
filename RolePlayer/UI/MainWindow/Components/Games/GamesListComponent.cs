namespace RolePlayer.UI.MainWindow.Components.Games;

using Dalamud.Bindings.ImGui;
using Dalamud.Interface;
using RolePlayer.Core.GameEngine.Contracts;
using RolePlayer.UI.GameDashboard.Contracts;
using RolePlayer.UI.GameDashboard.Windows;
using RolePlayer.UI.Localization.Contracts;
using RolePlayer.UI.MainWindow.Contracts;
using System.Linq;
using System.Numerics;

public class GamesListComponent {
    private IGameLibraryService libraryService;
    private IGameSelectionState selectionState;
    private ILocalizationService localization;
    private IGameDashboardPresenter dashboardPresenter;
    private GameDashboardWindow dashboardWindow;

    public GamesListComponent(
        IGameLibraryService libraryService,
        IGameSelectionState selectionState,
        ILocalizationService localization,
        IGameDashboardPresenter dashboardPresenter,
        GameDashboardWindow dashboardWindow) {

        this.libraryService = libraryService;
        this.selectionState = selectionState;
        this.localization = localization;
        this.dashboardPresenter = dashboardPresenter;
        this.dashboardWindow = dashboardWindow;
    }

    public void Draw() {
        ImGui.TextDisabled(this.localization.Translate("main_tab_games"));

        float availableWidth = ImGui.GetContentRegionAvail().X;
        float btnWidth = 150f;

        ImGui.SetCursorPosX(availableWidth - btnWidth);
        ImGui.PushFont(UiBuilder.IconFont);
        string plusIcon = FontAwesomeIcon.Plus.ToIconString();
        ImGui.PopFont();

        if (ImGui.Button($"{plusIcon} {this.localization.Translate("games_create_new")}", new Vector2(btnWidth, 0))) {
            this.selectionState.SelectedGame = null;
            this.selectionState.IsCreatingNew = true;
        }

        ImGui.Spacing();
        ImGui.Separator();
        ImGui.Spacing();

        var games = this.libraryService.GetAvailableGames().ToList();
        if (games.Count == 0) return;

        if (ImGui.BeginTable("GamesListTable", 3, ImGuiTableFlags.Borders | ImGuiTableFlags.RowBg | ImGuiTableFlags.SizingFixedFit)) {
            ImGui.TableSetupColumn(this.localization.Translate("config_common_name"), ImGuiTableColumnFlags.WidthStretch);
            ImGui.TableSetupColumn("Type", ImGuiTableColumnFlags.WidthFixed, 120f);
            ImGui.TableSetupColumn(this.localization.Translate("config_common_actions"), ImGuiTableColumnFlags.WidthFixed, 80f);
            ImGui.TableHeadersRow();

            foreach (var game in games) {
                float rowHeight = 32f;
                ImGui.TableNextRow(ImGuiTableRowFlags.None, rowHeight);

                bool isSelected = this.selectionState.SelectedGame?.Id == game.Id;

                ImGui.TableNextColumn();
                ImGui.PushStyleVar(ImGuiStyleVar.SelectableTextAlign, new Vector2(0.0f, 0.5f));
                float selectableHeight = rowHeight - (ImGui.GetStyle().CellPadding.Y * 2);

                if (ImGui.Selectable($"{game.Name}##sel_{game.Id}", isSelected, ImGuiSelectableFlags.SpanAllColumns | ImGuiSelectableFlags.AllowItemOverlap, new Vector2(0, selectableHeight))) {
                    this.selectionState.IsCreatingNew = false;
                    this.selectionState.SelectedGame = isSelected ? null : game;
                }
                ImGui.PopStyleVar();

                ImGui.TableNextColumn();
                ImGui.AlignTextToFramePadding();
                ImGui.TextDisabled(game.EngineType);

                ImGui.TableNextColumn();
                float buttonHeight = ImGui.GetFrameHeight();
                float startY = ImGui.GetCursorPosY() - ImGui.GetStyle().CellPadding.Y;
                ImGui.SetCursorPosY(startY + (rowHeight - buttonHeight) / 2f);

                ImGui.PushFont(UiBuilder.IconFont);
                ImGui.PushStyleColor(ImGuiCol.Button, new Vector4(0.2f, 0.6f, 0.2f, 1.0f));

                if (ImGui.Button($"{FontAwesomeIcon.Play.ToIconString()}##Host_{game.Id}")) {
                    this.dashboardPresenter.SelectGame(game);
                    this.dashboardWindow.OpenForGame();
                }

                ImGui.PopStyleColor();
                ImGui.PopFont();

                if (ImGui.IsItemHovered()) ImGui.SetTooltip(this.localization.Translate("games_host_session"));
            }
            ImGui.EndTable();
        }
    }
}