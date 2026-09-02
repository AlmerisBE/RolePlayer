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

    public IEnumerable<string> GetAvailableTags() {
        return this.configurationService.GetConfig().AvailableTags;
    }

    public void CreateGlobalTag(string tag) {
        if (string.IsNullOrWhiteSpace(tag)) {
            return;
        }

        var config = this.configurationService.GetConfig();
        if (config.AvailableTags.Add(tag.Trim())) {
            this.configurationService.Save();
        }
    }

    public void DeleteGlobalTag(string tag) {
        var config = this.configurationService.GetConfig();
        var changed = false;

        if (config.AvailableTags.Remove(tag)) {
            changed = true;
        }

        foreach (var kvp in config.EmoteTags) {
            if (kvp.Value.Remove(tag)) {
                changed = true;
            }
        }

        if (changed) {
            this.configurationService.Save();
        }
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