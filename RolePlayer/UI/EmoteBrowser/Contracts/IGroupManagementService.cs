namespace RolePlayer.UI.EmoteBrowser.Contracts;

using RolePlayer.Core.MetaData.Models;
using System.Collections.Generic;

public interface IGroupManagementService {
    IEnumerable<EmoteGroup> GetGroups();
    void CreateGroup(EmoteGroup group);
    void DeleteGroup(string groupName);
    string? GetGroupForEmote(uint emoteId);
    void AssignEmoteToGroup(uint emoteId, string groupName);
    void RemoveEmoteFromGroup(uint emoteId);
}