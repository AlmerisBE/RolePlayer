namespace RolePlayer.UI.Hotbar.Services;

using RolePlayer.UI.EmoteBrowser.Contracts;
using RolePlayer.UI.EmoteBrowser.Models;
using RolePlayer.UI.Hotbar.Contracts;
using RolePlayer.UI.Hotbar.Models;
using System.Collections.Generic;
using System.Linq;

public class HotbarResolverService : IHotbarResolverService {
    private IGroupManagementService groupManagementService;
    private ITagManagementService tagManagementService;

    public HotbarResolverService(IGroupManagementService groupManagementService, ITagManagementService tagManagementService) {
        this.groupManagementService = groupManagementService;
        this.tagManagementService = tagManagementService;
    }

    public List<EmoteDisplayData> ResolveEmotesForHotbar(HotbarConfig config, IEnumerable<EmoteDisplayData> allCachedEmotes) {
        if (config.PopulationMode == HotbarPopulationMode.Manual) {
            return allCachedEmotes.Where(e => config.ManualEmoteIds.Contains(e.Id)).ToList();
        }

        var results = new List<EmoteDisplayData>();
        var query = config.SearchQuery.Trim().ToLowerInvariant();
        bool hasSearch = !string.IsNullOrEmpty(query);
        bool hasCatFilter = config.SelectedCategories.Count > 0;
        bool hasGroupFilter = config.SelectedGroups.Count > 0;
        bool hasTagFilter = config.SelectedTags.Count > 0;

        foreach (var emote in allCachedEmotes) {
            if (config.ShowModdedOnly && !emote.IsModded) {
                continue;
            }

            if (hasSearch) {
                bool matchesName = emote.Name.ToLowerInvariant().Contains(query);
                bool matchesCmd = emote.LocalizedCommand.ToLowerInvariant().Contains(query);
                bool matchesEnCmd = !string.IsNullOrEmpty(emote.EnglishCommand) && emote.EnglishCommand.ToLowerInvariant().Contains(query);
                if (!matchesName && !matchesCmd && !matchesEnCmd) {
                    continue;
                }
            }

            if (hasCatFilter && !config.SelectedCategories.Contains(emote.Category)) {
                continue;
            }

            var customGroup = this.groupManagementService.GetGroupForEmote(emote.Id);
            if (hasGroupFilter && (string.IsNullOrEmpty(customGroup) || !config.SelectedGroups.Contains(customGroup))) {
                continue;
            }

            if (hasTagFilter) {
                var tags = this.tagManagementService.GetTagsForEmote(emote.Id);
                if (!config.SelectedTags.Overlaps(tags)) {
                    continue;
                }
            }

            results.Add(emote);
        }

        return results;
    }
}