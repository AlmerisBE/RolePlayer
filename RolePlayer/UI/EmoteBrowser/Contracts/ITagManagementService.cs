namespace RolePlayer.UI.EmoteBrowser.Contracts;

using System.Collections.Generic;

public interface ITagManagementService {
    IEnumerable<string> GetAvailableTags();
    IEnumerable<string> GetTagsForEmote(uint emoteId);
    void CreateGlobalTag(string tag);
    void DeleteGlobalTag(string tag);
    void AddTagToEmote(uint emoteId, string tag);
    void RemoveTagFromEmote(uint emoteId, string tag);
}