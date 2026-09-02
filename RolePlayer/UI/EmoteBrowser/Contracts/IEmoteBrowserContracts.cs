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
    bool IsEmoteModded(uint emoteId);
}

public interface ITagManagementService {
    IEnumerable<string> GetTagsForEmote(uint emoteId);
    void AddTag(uint emoteId, string tag);
    void RemoveTag(uint emoteId, string tag);
}