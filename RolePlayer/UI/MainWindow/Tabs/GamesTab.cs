namespace RolePlayer.UI.MainWindow.Tabs;

using RolePlayer.UI.EmoteBrowser.Contracts;
using RolePlayer.UI.Localization.Contracts;
using RolePlayer.UI.MainWindow.Components.Games;
using RolePlayer.UI.MainWindow.Contracts;
using System;

public class GamesTab : IEmoteBrowserTab, IDisposable {
    private ILocalizationService localization;
    private IGameSelectionState selectionState;
    private GamesListComponent listComponent;
    private GameEditorPanelComponent editorComponent;

    public string TabName => this.localization.Translate("main_tab_games");
    public int SortOrder => 30;

    public bool IsSidePanelOpen => this.selectionState.SelectedGame != null || this.selectionState.IsCreatingNew;

    public GamesTab(
        ILocalizationService localization,
        IGameSelectionState selectionState,
        GamesListComponent listComponent,
        GameEditorPanelComponent editorComponent) {

        this.localization = localization;
        this.selectionState = selectionState;
        this.listComponent = listComponent;
        this.editorComponent = editorComponent;
    }

    public void Draw() {
        this.listComponent.Draw();
    }

    public void DrawSidePanel() {
        this.editorComponent.Draw();
    }

    public void Dispose() { }
}