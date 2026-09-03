namespace RolePlayer.Core.MetaData.Services;

using RolePlayer.Core.Configuration.Contracts;
using RolePlayer.Core.MetaData.Models;
using RolePlayer.UI.EmoteBrowser.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;

public class GroupManagementService : IGroupManagementService {
    private IConfigurationService configurationService;

    public GroupManagementService(IConfigurationService configurationService) {
        this.configurationService = configurationService;
    }

    public IEnumerable<EmoteGroup> GetGroups() => this.configurationService.GetConfig().EmoteGroups;

    public void CreateGroup(EmoteGroup group) {
        if (group == null || string.IsNullOrWhiteSpace(group.Name)) {
            return;
        }

        var config = this.configurationService.GetConfig();
        if (config.EmoteGroups.Any(g => g.Name.Equals(group.Name, StringComparison.OrdinalIgnoreCase))) {
            return;
        }

        config.EmoteGroups.Add(group);
        this.configurationService.Save();
    }

    public void UpdateGroup(string oldName, string newName, string description) {
        if (string.IsNullOrWhiteSpace(newName)) {
            return;
        }

        var config = this.configurationService.GetConfig();
        var group = config.EmoteGroups.FirstOrDefault(g => g.Name.Equals(oldName, StringComparison.OrdinalIgnoreCase));
        if (group == null) {
            return;
        }

        bool nameChanged = !oldName.Equals(newName, StringComparison.OrdinalIgnoreCase);
        if (nameChanged && config.EmoteGroups.Any(g => g.Name.Equals(newName, StringComparison.OrdinalIgnoreCase))) {
            return;
        }

        group.Name = newName;
        group.Description = description;

        if (nameChanged) {
            var keysToUpdate = config.EmoteToGroupMap.Where(kvp => kvp.Value.Equals(oldName, StringComparison.OrdinalIgnoreCase)).Select(kvp => kvp.Key).ToList();
            foreach (var key in keysToUpdate) {
                config.EmoteToGroupMap[key] = newName;
            }
        }

        this.configurationService.Save();
    }

    public void DeleteGroup(string groupName) {
        var config = this.configurationService.GetConfig();
        var removed = config.EmoteGroups.RemoveAll(g => g.Name.Equals(groupName, StringComparison.OrdinalIgnoreCase));

        var keysToRemove = config.EmoteToGroupMap.Where(kvp => kvp.Value.Equals(groupName, StringComparison.OrdinalIgnoreCase)).Select(kvp => kvp.Key).ToList();
        foreach (var key in keysToRemove) {
            config.EmoteToGroupMap.Remove(key);
        }

        if (removed > 0 || keysToRemove.Count > 0) {
            this.configurationService.Save();
        }
    }

    public string? GetGroupForEmote(uint emoteId) {
        var config = this.configurationService.GetConfig();
        if (config.EmoteToGroupMap.TryGetValue(emoteId, out var groupName)) {
            return groupName;
        }

        return null;
    }

    public void AssignEmoteToGroup(uint emoteId, string groupName) {
        var config = this.configurationService.GetConfig();
        config.EmoteToGroupMap[emoteId] = groupName;
        this.configurationService.Save();
    }

    public void RemoveEmoteFromGroup(uint emoteId) {
        var config = this.configurationService.GetConfig();
        if (config.EmoteToGroupMap.Remove(emoteId)) {
            this.configurationService.Save();
        }
    }

    public int GetGroupEmoteCount(string groupName) {
        return this.configurationService.GetConfig().EmoteToGroupMap.Values.Count(v => v.Equals(groupName, StringComparison.OrdinalIgnoreCase));
    }
}