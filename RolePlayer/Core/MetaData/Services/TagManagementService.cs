namespace RolePlayer.Core.MetaData.Services;

using RolePlayer.Core.Configuration.Contracts;
using RolePlayer.UI.EmoteBrowser.Contracts;
using System.Collections.Generic;
using System.Linq;

public class TagManagementService : ITagManagementService {
    private IConfigurationService configurationService;

    public TagManagementService(IConfigurationService configurationService) {
        this.configurationService = configurationService;
    }

    public IEnumerable<string> GetTags() {
        var config = this.configurationService.GetConfig();
        return config.EmoteTags.Values.SelectMany(tags => tags).Distinct();
    }

    public IEnumerable<string> GetTagsForEmote(uint emoteId) {
        var config = this.configurationService.GetConfig();
        if (config.EmoteTags.TryGetValue(emoteId, out var tags)) {
            return tags;
        }

        return Enumerable.Empty<string>();
    }

    public void AddTagToEmote(uint emoteId, string tag) {
        if (string.IsNullOrWhiteSpace(tag)) {
            return;
        }

        var config = this.configurationService.GetConfig();
        if (!config.EmoteTags.ContainsKey(emoteId)) {
            config.EmoteTags[emoteId] = new HashSet<string>();
        }

        if (config.EmoteTags[emoteId].Add(tag)) {
            this.configurationService.Save();
        }
    }

    public void RemoveTagFromEmote(uint emoteId, string tag) {
        var config = this.configurationService.GetConfig();
        if (config.EmoteTags.TryGetValue(emoteId, out var tags) && tags.Remove(tag)) {
            this.configurationService.Save();
        }
    }
}