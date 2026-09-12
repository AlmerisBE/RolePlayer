namespace RolePlayer.UI.EmoteBrowser.Presenters;

using RolePlayer.Core.Configuration.Contracts;
using RolePlayer.Core.Emotes.Contracts;
using RolePlayer.Core.Emotes.Models;
using RolePlayer.UI.EmoteBrowser.Contracts;
using RolePlayer.UI.EmoteBrowser.Models;
using RolePlayer.UI.Localization.Contracts;
using System.Collections.Generic;
using System.Linq;

public class EmoteBrowserPresenter : IEmoteBrowserPresenter {
    private IEmoteCache emoteCache;
    private IContextManagementService contextService;
    private IGroupManagementService groupManagementService;
    private ITagManagementService tagManagementService;
    private ILocalizationService localization;

    private List<string> availableCategories = new();
    private Dictionary<string, IReadOnlyList<EnrichedEmote>> groupedEmotes = new();

    private string searchQuery = string.Empty;
    private int sortColumn = -1;
    private bool sortDescending = false;

    public bool IsLoading => !this.emoteCache.IsReady;
    public IReadOnlyList<string> AvailableCategories => this.availableCategories;
    public IReadOnlyDictionary<string, IReadOnlyList<EnrichedEmote>> GroupedEmotes => this.groupedEmotes;

    public string SearchQuery {
        get => this.searchQuery;
        set {
            if (this.searchQuery == value) return;
            this.searchQuery = value;
            this.ApplyFilters();
        }
    }

    public EmoteBrowserPresenter(
        IEmoteCache emoteCache,
        IContextManagementService contextService,
        IGroupManagementService groupManagementService,
        ITagManagementService tagManagementService,
        ILocalizationService localization) {

        this.emoteCache = emoteCache;
        this.contextService = contextService;
        this.groupManagementService = groupManagementService;
        this.tagManagementService = tagManagementService;
        this.localization = localization;
    }

    public void Initialize() {
        this.emoteCache.CacheUpdated += this.Refresh;
        this.Refresh();
    }

    public void Refresh() {
        if (!this.emoteCache.IsReady) return;

        var emotes = this.emoteCache.GetCachedEmotes();
        var uniqueCategories = new HashSet<string>();

        foreach (var emote in emotes) {
            if (!string.IsNullOrEmpty(emote.Category)) uniqueCategories.Add(emote.Category);
        }

        this.availableCategories = uniqueCategories.OrderBy(c => c).ToList();
        this.ApplyFilters();
    }

    public void ApplyFilters() {
        var emotes = this.emoteCache.GetCachedEmotes();
        var newGroupedEmotes = new Dictionary<string, List<EnrichedEmote>>();
        var context = this.contextService.GetCurrentContext();

        var query = this.searchQuery.Trim().ToLowerInvariant();
        bool hasSearch = !string.IsNullOrEmpty(query);
        bool hasCatFilter = context.SelectedCategories.Count > 0;
        bool hasGroupFilter = context.SelectedGroups.Count > 0;
        bool hasTagFilter = context.SelectedTags.Count > 0;

        foreach (var emote in emotes) {
            if (context.ShowModdedOnly && !emote.IsModded) continue;

            if (context.UnlockFilter == UnlockFilterMode.Unlocked && !emote.IsUnlocked) continue;
            if (context.UnlockFilter == UnlockFilterMode.Locked && emote.IsUnlocked) continue;

            if (hasSearch) {
                bool matchesName = emote.Name.ToLowerInvariant().Contains(query);
                bool matchesCmd = emote.LocalizedCommand.ToLowerInvariant().Contains(query);
                bool matchesEnCmd = !string.IsNullOrEmpty(emote.EnglishCommand) && emote.EnglishCommand.ToLowerInvariant().Contains(query);
                if (!matchesName && !matchesCmd && !matchesEnCmd) continue;
            }

            if (hasCatFilter && !context.SelectedCategories.Contains(emote.Category)) continue;

            var customGroup = this.groupManagementService.GetGroupForEmote(emote.Id);
            if (hasGroupFilter && (string.IsNullOrEmpty(customGroup) || !context.SelectedGroups.Contains(customGroup))) continue;

            if (hasTagFilter) {
                var tags = this.tagManagementService.GetTagsForEmote(emote.Id);
                if (!context.SelectedTags.Overlaps(tags)) continue;
            }

            string groupKey = this.localization.Translate("browser_all");

            if (context.CurrentGrouping == GroupingMode.NativeCategory) groupKey = string.IsNullOrEmpty(emote.Category) ? this.localization.Translate("browser_uncategorized") : emote.Category;
            else if (context.CurrentGrouping == GroupingMode.CustomGroup) groupKey = string.IsNullOrEmpty(customGroup) ? this.localization.Translate("browser_ungrouped") : customGroup;

            if (!newGroupedEmotes.ContainsKey(groupKey)) newGroupedEmotes[groupKey] = new List<EnrichedEmote>();

            newGroupedEmotes[groupKey].Add(emote);
        }

        var finalizedDictionary = new Dictionary<string, IReadOnlyList<EnrichedEmote>>();

        foreach (var key in newGroupedEmotes.Keys.ToList()) {
            var list = newGroupedEmotes[key].AsEnumerable();

            if (this.sortColumn == 1) list = this.sortDescending ? list.OrderByDescending(e => e.Name) : list.OrderBy(e => e.Name);
            else if (this.sortColumn == 2) list = this.sortDescending ? list.OrderByDescending(e => e.LocalizedCommand) : list.OrderBy(e => e.LocalizedCommand);

            finalizedDictionary[key] = list.ToList();
        }

        this.groupedEmotes = finalizedDictionary;
    }

    public void SetSort(int columnIndex, bool descending) {
        if (this.sortColumn == columnIndex && this.sortDescending == descending) return;

        this.sortColumn = columnIndex;
        this.sortDescending = descending;
        this.ApplyFilters();
    }

    public void Dispose() {
        this.emoteCache.CacheUpdated -= this.Refresh;
    }
}