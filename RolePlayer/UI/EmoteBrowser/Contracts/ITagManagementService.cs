namespace RolePlayer.UI.EmoteBrowser.Contracts;

using System;
using System.Collections.Generic;

public interface ITagManagementService {
    IEnumerable<string> GetAvailableTags();
    void CreateGlobalTag(string tag);
    void RenameGlobalTag(string oldTag, string newTag);
    void DeleteGlobalTag(string tag);

    IEnumerable<string> GetTagsForEmote(uint emoteId);
    void AddTagToEmote(uint emoteId, string tag);
    void RemoveTagFromEmote(uint emoteId, string tag);

    IEnumerable<string> GetTagsForMacro(Guid macroId);
    void AddTagToMacro(Guid macroId, string tag);
    void RemoveTagFromMacro(Guid macroId, string tag);

    int GetTagEmoteCount(string tag);
}