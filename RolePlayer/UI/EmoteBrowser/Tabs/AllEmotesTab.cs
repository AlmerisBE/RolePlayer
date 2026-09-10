namespace RolePlayer.UI.EmoteBrowser.Tabs;

using RolePlayer.Core.Configuration.Contracts;
using RolePlayer.UI.EmoteBrowser.Components;
using RolePlayer.UI.EmoteBrowser.Contracts;
using RolePlayer.UI.Localization.Contracts;
using System;

public class AllEmotesTab : IEmoteBrowserTab, IDisposable {
    private IEmoteBrowserPresenter presenter;
    private IEmoteSelectionState selectionState;
    private IContextManagementService contextService;
    private EmoteFilterComponent filterComponent;
    private EmoteListComponent listComponent;
    private EmoteDetailsPanel detailsPanel;
    private ILocalizationService localization;

    public string TabName => this.localization.Translate("browser_tab_all_emotes");
    public int SortOrder => 0;
    public bool IsSidePanelOpen => this.selectionState.SelectedEmote != null;

    public AllEmotesTab(
        IEmoteBrowserPresenter presenter,
        IEmoteSelectionState selectionState,
        IContextManagementService contextService,
        EmoteFilterComponent filterComponent,
        EmoteListComponent listComponent,
        EmoteDetailsPanel detailsPanel,
        ILocalizationService localization) {

        this.presenter = presenter;
        this.selectionState = selectionState;
        this.contextService = contextService;
        this.filterComponent = filterComponent;
        this.listComponent = listComponent;
        this.detailsPanel = detailsPanel;
        this.localization = localization;

        this.presenter.Initialize();
    }

    public void Draw() {
        this.filterComponent.Draw(this.presenter);

        var context = this.contextService.GetCurrentContext();
        this.listComponent.Draw(this.presenter, context);
    }

    public void DrawSidePanel() => this.detailsPanel.Draw();

    public void Dispose() => this.presenter.Dispose();
}