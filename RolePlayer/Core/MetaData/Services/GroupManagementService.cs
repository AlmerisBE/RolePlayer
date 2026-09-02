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

    public IEnumerable<EmoteGroup> GetGroups() {
        return this.configurationService.GetConfig().EmoteGroups;
    }

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

    public void DeleteGroup(string groupName) {
        var config = this.configurationService.GetConfig();
        var removed = config.EmoteGroups.RemoveAll(g => g.Name.Equals(groupName, StringComparison.OrdinalIgnoreCase));

        if (removed > 0) {
            this.configurationService.Save();
        }
    }
}