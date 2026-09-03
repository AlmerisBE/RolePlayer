namespace RolePlayer.Core.MetaData.Services;

using RolePlayer.Core.Configuration.Contracts;
using RolePlayer.UI.EmoteBrowser.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;

public class TagManagementService : ITagManagementService {
    private IContextManagementService contextService;
    private IConfigurationService configurationService;

    public TagManagementService(IContextManagementService contextService, IConfigurationService configurationService) {
        this.contextService = contextService;
        this.configurationService = configurationService;
    }

    public IEnumerable<string> GetAvailableTags() {
        var context = this.contextService.GetCurrentContext();
        return context.AvailableTags ?? Enumerable.Empty<string>();
    }

    public void CreateGlobalTag(string tag) {
        if (string.IsNullOrWhiteSpace(tag)) {
            return;
        }

        var context = this.contextService.GetCurrentContext();
        if (context.AvailableTags.Add(tag.Trim())) {
            this.configurationService.Save();
        }
    }

    public void RenameGlobalTag(string oldTag, string newTag) {
        if (string.IsNullOrWhiteSpace(newTag) || oldTag.Equals(newTag, StringComparison.OrdinalIgnoreCase)) {
            return;
        }

        var context = this.contextService.GetCurrentContext();
        if (context.AvailableTags.Contains(newTag)) {
            return;
        }

        if (context.AvailableTags.Remove(oldTag)) {
            context.AvailableTags.Add(newTag.Trim());
        }

        foreach (var kvp in context.EmoteTags) {
            if (kvp.Value.Remove(oldTag)) {
                kvp.Value.Add(newTag.Trim());
            }
        }

        this.configurationService.Save();
    }

    public void DeleteGlobalTag(string tag) {
        var context = this.contextService.GetCurrentContext();
        bool changed = context.AvailableTags.Remove(tag);

        foreach (var kvp in context.EmoteTags) {
            if (kvp.Value.Remove(tag)) {
                changed = true;
            }
        }

        if (changed) {
            this.configurationService.Save();
        }
    }

    public IEnumerable<string> GetTagsForEmote(uint emoteId) {
        var context = this.contextService.GetCurrentContext();
        if (context.EmoteTags.TryGetValue(emoteId, out var tags)) {
            return tags;
        }

        return Enumerable.Empty<string>();
    }

    public void AddTagToEmote(uint emoteId, string tag) {
        if (string.IsNullOrWhiteSpace(tag)) {
            return;
        }

        var context = this.contextService.GetCurrentContext();
        if (!context.EmoteTags.ContainsKey(emoteId)) {
            context.EmoteTags[emoteId] = new HashSet<string>();
        }

        if (context.EmoteTags[emoteId].Add(tag)) {
            this.configurationService.Save();
        }
    }

    public void RemoveTagFromEmote(uint emoteId, string tag) {
        var context = this.contextService.GetCurrentContext();
        if (context.EmoteTags.TryGetValue(emoteId, out var tags) && tags.Remove(tag)) {
            this.configurationService.Save();
        }
    }

    public int GetTagEmoteCount(string tag) {
        return this.contextService.GetCurrentContext().EmoteTags.Values.Count(tags => tags.Contains(tag));
    }
}