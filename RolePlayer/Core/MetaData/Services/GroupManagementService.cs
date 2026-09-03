namespace RolePlayer.Core.MetaData.Services;

using RolePlayer.Core.Configuration.Contracts;
using RolePlayer.Core.MetaData.Models;
using RolePlayer.UI.EmoteBrowser.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;

public class GroupManagementService : IGroupManagementService {
    private IContextManagementService contextService;
    private IConfigurationService configurationService;

    public GroupManagementService(IContextManagementService contextService, IConfigurationService configurationService) {
        this.contextService = contextService;
        this.configurationService = configurationService;
    }

    public IEnumerable<EmoteGroup> GetGroups() => this.contextService.GetCurrentContext().EmoteGroups;

    public void CreateGroup(EmoteGroup group) {
        if (group == null || string.IsNullOrWhiteSpace(group.Name)) {
            return;
        }

        var context = this.contextService.GetCurrentContext();
        if (context.EmoteGroups.Any(g => g.Name.Equals(group.Name, StringComparison.OrdinalIgnoreCase))) {
            return;
        }

        context.EmoteGroups.Add(group);
        this.configurationService.Save();
    }

    public void UpdateGroup(string oldName, string newName, string description) {
        if (string.IsNullOrWhiteSpace(newName)) {
            return;
        }

        var context = this.contextService.GetCurrentContext();
        var group = context.EmoteGroups.FirstOrDefault(g => g.Name.Equals(oldName, StringComparison.OrdinalIgnoreCase));
        if (group == null) {
            return;
        }

        bool nameChanged = !oldName.Equals(newName, StringComparison.OrdinalIgnoreCase);
        if (nameChanged && context.EmoteGroups.Any(g => g.Name.Equals(newName, StringComparison.OrdinalIgnoreCase))) {
            return;
        }

        group.Name = newName;
        group.Description = description;

        if (nameChanged) {
            var keysToUpdate = context.EmoteToGroupMap.Where(kvp => kvp.Value.Equals(oldName, StringComparison.OrdinalIgnoreCase)).Select(kvp => kvp.Key).ToList();
            foreach (var key in keysToUpdate) {
                context.EmoteToGroupMap[key] = newName;
            }
        }

        this.configurationService.Save();
    }

    public void DeleteGroup(string groupName) {
        var context = this.contextService.GetCurrentContext();
        var removed = context.EmoteGroups.RemoveAll(g => g.Name.Equals(groupName, StringComparison.OrdinalIgnoreCase));

        var keysToRemove = context.EmoteToGroupMap.Where(kvp => kvp.Value.Equals(groupName, StringComparison.OrdinalIgnoreCase)).Select(kvp => kvp.Key).ToList();
        foreach (var key in keysToRemove) {
            context.EmoteToGroupMap.Remove(key);
        }

        if (removed > 0 || keysToRemove.Count > 0) {
            this.configurationService.Save();
        }
    }

    public string? GetGroupForEmote(uint emoteId) {
        var context = this.contextService.GetCurrentContext();
        if (context.EmoteToGroupMap.TryGetValue(emoteId, out var groupName)) {
            return groupName;
        }

        return null;
    }

    public void AssignEmoteToGroup(uint emoteId, string groupName) {
        var context = this.contextService.GetCurrentContext();
        context.EmoteToGroupMap[emoteId] = groupName;
        this.configurationService.Save();
    }

    public void RemoveEmoteFromGroup(uint emoteId) {
        var context = this.contextService.GetCurrentContext();
        if (context.EmoteToGroupMap.Remove(emoteId)) {
            this.configurationService.Save();
        }
    }

    public int GetGroupEmoteCount(string groupName) {
        return this.contextService.GetCurrentContext().EmoteToGroupMap.Values.Count(v => v.Equals(groupName, StringComparison.OrdinalIgnoreCase));
    }
}