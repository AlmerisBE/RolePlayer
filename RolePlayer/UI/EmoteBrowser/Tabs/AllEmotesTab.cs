namespace RolePlayer.UI.EmoteBrowser.Tabs;

using RolePlayer.Core.Configuration.Contracts;
using RolePlayer.Core.Logging.Contracts;
using RolePlayer.UI.EmoteBrowser.Components;
using RolePlayer.UI.EmoteBrowser.Contracts;
using RolePlayer.UI.EmoteBrowser.Models;
using RolePlayer.UI.Localization.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public class AllEmotesTab : IEmoteBrowserTab, IDisposable {
    private IEmoteRepository emoteRepository;
    private IPlayerStateProvider playerStateProvider;
    private IEmoteSelectionState selectionState;
    private IModStateProvider modStateProvider;
    private ILoggerService logger;
    private IContextManagementService contextService;
    private EmoteFilterComponent filterComponent;
    private EmoteListComponent listComponent;
    private EmoteDetailsPanel detailsPanel;
    private ILocalizationService localization;

    private List<EmoteDisplayData> emotesCache = new();
    private List<string> availableCategories = new();
    private Dictionary<string, List<EmoteDisplayData>> groupedEmotes = new();

    private bool needsRefresh = false;
    private bool needsFilterApply = false;
    private bool isRefreshing = false;

    public string TabName => this.localization.Translate("browser_tab_all_emotes");
    public int SortOrder => 0;
    public bool IsSidePanelOpen => this.selectionState.SelectedEmote != null;

    public AllEmotesTab(
        IEmoteRepository emoteRepository,
        IPlayerStateProvider playerStateProvider,
        IEmoteSelectionState selectionState,
        IModStateProvider modStateProvider,
        ILoggerService logger,
        IContextManagementService contextService,
        EmoteFilterComponent filterComponent,
        EmoteListComponent listComponent,
        EmoteDetailsPanel detailsPanel,
        ILocalizationService localization) {

        this.emoteRepository = emoteRepository;
        this.playerStateProvider = playerStateProvider;
        this.selectionState = selectionState;
        this.modStateProvider = modStateProvider;
        this.logger = logger;
        this.contextService = contextService;
        this.filterComponent = filterComponent;
        this.listComponent = listComponent;
        this.detailsPanel = detailsPanel;
        this.localization = localization;

        this.modStateProvider.ModStateChanged += this.OnModStateChanged;
        this.playerStateProvider.PlayerStateValid += this.OnPlayerStateValid;
    }

    private void OnModStateChanged() => this.needsRefresh = true;
    private void OnPlayerStateValid() => this.needsRefresh = true;

    public void Draw() {
        if (this.needsRefresh && !this.isRefreshing) {
            this.needsRefresh = false;
            this.LoadEmotesAsync();
        }

        if (!this.emotesCache.Any() && !this.isRefreshing) this.LoadEmotesAsync();

        bool filtersChanged = this.filterComponent.Draw(this.availableCategories);

        if (filtersChanged || this.needsFilterApply) {
            this.groupedEmotes = this.filterComponent.Apply(this.emotesCache);
            this.needsFilterApply = false;
        }

        var context = this.contextService.GetCurrentContext();
        bool listTriggeredFilterUpdate = this.listComponent.Draw(
            this.groupedEmotes,
            context,
            (col, desc) => this.filterComponent.RegisterSort(col, desc)
        );

        if (listTriggeredFilterUpdate) this.needsFilterApply = true;
    }

    private void LoadEmotesAsync() {
        if (!this.playerStateProvider.IsPlayerValid) return;

        this.isRefreshing = true;
        Task.Run(() => {
            try {
                var baseEmotes = this.emoteRepository.GetBaseEmotes().ToList();
                var uniqueCategories = new HashSet<string>();
                var newCache = new List<EmoteDisplayData>();

                foreach (var emote in baseEmotes) {
                    emote.IsUnlocked = !emote.IsUnlockable || this.playerStateProvider.IsEmoteUnlocked(emote.Id);
                    emote.IsModded = !string.IsNullOrEmpty(this.modStateProvider.GetModNameModifyingEmote(emote.Id));

                    newCache.Add(emote);
                    if (!string.IsNullOrEmpty(emote.Category)) uniqueCategories.Add(emote.Category);
                }

                this.emotesCache = newCache;
                this.availableCategories = uniqueCategories.OrderBy(c => c).ToList();
                this.needsFilterApply = true;
            }
            catch (Exception ex) {
                this.logger.Error(ex, "[AllEmotesTab] Background emote resolution failed unexpectedly.");
            }
            finally {
                this.isRefreshing = false;
            }
        });
    }

    public void DrawSidePanel() => this.detailsPanel.Draw();

    public void Dispose() {
        this.modStateProvider.ModStateChanged -= this.OnModStateChanged;
        this.playerStateProvider.PlayerStateValid -= this.OnPlayerStateValid;
    }
}