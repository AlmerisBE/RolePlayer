namespace RolePlayer.UI.EmoteBrowser.Contracts;

using System.Collections.Generic;

public interface ITagManagementService {
    IEnumerable<string> GetTags();
    IEnumerable<string> GetTagsForEmote(uint emoteId);
    void AddTagToEmote(uint emoteId, string tag);
    void RemoveTagFromEmote(uint emoteId, string tag);
    void DeleteTag(string tag);
}