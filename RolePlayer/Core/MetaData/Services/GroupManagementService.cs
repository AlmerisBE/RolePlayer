namespace RolePlayer.Core.MetaData.Services;

using RolePlayer.Core.Configuration.Contracts;
using RolePlayer.Core.MetaData.Models;
using RolePlayer.UI.EmoteBrowser.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;

public class GroupManagementService : IGroupManagementService {
    private IContextManagementService contextService;
    private IConfigurationService configService;

    public GroupManagementService(IContextManagementService contextService, IConfigurationService configService) {
        this.contextService = contextService;
        this.configService = configService;
    }

    public IEnumerable<EmoteGroup> GetGroups() {
        return this.contextService.GetCurrentContext().EmoteGroups;
    }

    public void CreateGroup(EmoteGroup group) {
        if (group == null || string.IsNullOrWhiteSpace(group.Name)) return;

        var context = this.contextService.GetCurrentContext();
        if (context.EmoteGroups.Any(g => g.Name.Equals(group.Name, StringComparison.OrdinalIgnoreCase))) return;

        context.EmoteGroups.Add(group);
        this.configService.Save();
    }

    public void UpdateGroup(string oldName, string newName, string description) {
        var context = this.contextService.GetCurrentContext();
        var group = context.EmoteGroups.FirstOrDefault(g => g.Name.Equals(oldName, StringComparison.OrdinalIgnoreCase));
        if (group == null) return;

        group.Name = newName;
        group.Description = description;
        this.configService.Save();
    }

    public void DeleteGroup(string groupName) {
        var context = this.contextService.GetCurrentContext();
        if (context.EmoteGroups.RemoveAll(g => g.Name.Equals(groupName, StringComparison.OrdinalIgnoreCase)) > 0) {
            this.configService.Save();
        }
    }

    public string? GetGroupForEmote(uint emoteId) {
        var context = this.contextService.GetCurrentContext();
        return context.EmoteToGroupMap.TryGetValue(emoteId, out var group) ? group : null;
    }

    public void AssignEmoteToGroup(uint emoteId, string groupName) {
        var context = this.contextService.GetCurrentContext();
        context.EmoteToGroupMap[emoteId] = groupName;
        this.configService.Save();
    }

    public void RemoveEmoteFromGroup(uint emoteId) {
        var context = this.contextService.GetCurrentContext();
        if (context.EmoteToGroupMap.Remove(emoteId)) this.configService.Save();
    }

    public string? GetGroupForMacro(Guid macroId) {
        var context = this.contextService.GetCurrentContext();
        return context.MacroToGroupMap.TryGetValue(macroId, out var group) ? group : null;
    }

    public void AssignMacroToGroup(Guid macroId, string groupName) {
        var context = this.contextService.GetCurrentContext();
        context.MacroToGroupMap[macroId] = groupName;
        this.configService.Save();
    }

    public void RemoveMacroFromGroup(Guid macroId) {
        var context = this.contextService.GetCurrentContext();
        if (context.MacroToGroupMap.Remove(macroId)) this.configService.Save();
    }

    public int GetGroupEmoteCount(string groupName) {
        return this.contextService.GetCurrentContext().EmoteToGroupMap.Values.Count(g => g.Equals(groupName, StringComparison.OrdinalIgnoreCase));
    }
}