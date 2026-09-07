namespace RolePlayer.UI.EmoteBrowser.Contracts;

using RolePlayer.Core.MetaData.Models;
using System;
using System.Collections.Generic;

public interface IGroupManagementService {
    IEnumerable<EmoteGroup> GetGroups();
    void CreateGroup(EmoteGroup group);
    void UpdateGroup(string oldName, string newName, string description);
    void DeleteGroup(string groupName);

    string? GetGroupForEmote(uint emoteId);
    void AssignEmoteToGroup(uint emoteId, string groupName);
    void RemoveEmoteFromGroup(uint emoteId);

    string? GetGroupForMacro(Guid macroId);
    void AssignMacroToGroup(Guid macroId, string groupName);
    void RemoveMacroFromGroup(Guid macroId);

    int GetGroupEmoteCount(string groupName);
}