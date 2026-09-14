namespace RolePlayer.UI.MainWindow.Tabs;

using RolePlayer.UI.EmoteBrowser.Contracts;
using RolePlayer.UI.Localization.Contracts;
using RolePlayer.UI.MainWindow.Components.Games;
using System;

public class GamesTab : IEmoteBrowserTab, IDisposable {
    private ILocalizationService localization;
    private GamesListComponent listComponent;

    public string TabName => this.localization.Translate("main_tab_games");
    public int SortOrder => 30;

    public bool IsSidePanelOpen => false;

    public GamesTab(ILocalizationService localization, GamesListComponent listComponent) {
        this.localization = localization;
        this.listComponent = listComponent;
    }

    public void Draw() {
        this.listComponent.Draw();
    }

    public void DrawSidePanel() { }

    public void Dispose() { }
}