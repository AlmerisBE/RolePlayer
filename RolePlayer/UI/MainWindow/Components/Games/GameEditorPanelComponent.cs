namespace RolePlayer.UI.MainWindow.Components.Games;

using Dalamud.Bindings.ImGui;
using Dalamud.Interface;
using RolePlayer.UI.Localization.Contracts;
using RolePlayer.UI.MainWindow.Contracts;

public class GameEditorPanelComponent {
    private IGameSelectionState selectionState;
    private ILocalizationService localization;

    public GameEditorPanelComponent(IGameSelectionState selectionState, ILocalizationService localization) {
        this.selectionState = selectionState;
        this.localization = localization;
    }

    public void Draw() {
        string closeIcon = FontAwesomeIcon.Times.ToIconString();
        ImGui.PushFont(UiBuilder.IconFont);
        var closeBtnWidth = ImGui.CalcTextSize(closeIcon).X + ImGui.GetStyle().FramePadding.X * 2;
        ImGui.PopFont();

        if (ImGui.BeginTable("GameEditorHeaderTable", 2)) {
            ImGui.TableSetupColumn("Title", ImGuiTableColumnFlags.WidthStretch);
            ImGui.TableSetupColumn("CloseBtn", ImGuiTableColumnFlags.WidthFixed, closeBtnWidth);

            ImGui.TableNextRow();
            ImGui.TableNextColumn();
            ImGui.AlignTextToFramePadding();
            ImGui.SetWindowFontScale(1.3f);

            string title = this.selectionState.IsCreatingNew ? this.localization.Translate("games_create_new") : this.selectionState.SelectedGame?.Name ?? string.Empty;
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

        ImGui.TextDisabled(this.localization.Translate("games_placeholder_editor"));
    }
}