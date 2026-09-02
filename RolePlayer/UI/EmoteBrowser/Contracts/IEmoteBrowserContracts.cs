namespace RolePlayer.UI.EmoteBrowser.Contracts;

using RolePlayer.UI.EmoteBrowser.Models;
using System.Collections.Generic;

public interface IEmoteRepository {
    IEnumerable<EmoteDisplayData> GetBaseEmotes();
}

public interface IPlayerStateProvider {
    bool IsEmoteUnlocked(uint emoteId);
}

public interface IModStateProvider {
    string GetModNameModifyingEmote(uint emoteId);
}

public interface ITagManagementService {
    IEnumerable<string> GetTags();
    IEnumerable<string> GetTagsForEmote(uint emoteId);
    void AddTagToEmote(uint emoteId, string tag);
    void RemoveTagFromEmote(uint emoteId, string tag);
}

public interface IGroupManagementService {
    IEnumerable<EmoteGroup> GetGroups();
    void CreateGroup(EmoteGroup group);
    void DeleteGroup(string groupName);
}

public interface IUnlockSourceProvider {
    string GetUnlockSource(uint emoteId);
}

public interface IEmoteBrowserTab {
    string TabName { get; }
    void Draw();
}