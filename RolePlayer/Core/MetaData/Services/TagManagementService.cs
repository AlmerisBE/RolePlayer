namespace RolePlayer.Core.MetaData.Services;

using RolePlayer.Core.Configuration.Contracts;
using RolePlayer.UI.EmoteBrowser.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;

public class TagManagementService : ITagManagementService {
    private IConfigurationService configurationService;

    public TagManagementService(IConfigurationService configurationService) {
        this.configurationService = configurationService;
    }

    public IEnumerable<string> GetAvailableTags() {
        var config = this.configurationService.GetConfig();
        return config.AvailableTags ?? Enumerable.Empty<string>();
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

    public void RenameGlobalTag(string oldTag, string newTag) {
        if (string.IsNullOrWhiteSpace(newTag) || oldTag.Equals(newTag, StringComparison.OrdinalIgnoreCase)) {
            return;
        }

        var config = this.configurationService.GetConfig();
        if (config.AvailableTags.Contains(newTag)) {
            return;
        }

        if (config.AvailableTags.Remove(oldTag)) {
            config.AvailableTags.Add(newTag.Trim());
        }

        foreach (var kvp in config.EmoteTags) {
            if (kvp.Value.Remove(oldTag)) {
                kvp.Value.Add(newTag.Trim());
            }
        }

        this.configurationService.Save();
    }

    public void DeleteGlobalTag(string tag) {
        var config = this.configurationService.GetConfig();
        bool changed = config.AvailableTags.Remove(tag);

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

    public int GetTagEmoteCount(string tag) {
        return this.configurationService.GetConfig().EmoteTags.Values.Count(tags => tags.Contains(tag));
    }
}