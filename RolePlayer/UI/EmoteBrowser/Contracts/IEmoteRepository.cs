namespace RolePlayer.UI.EmoteBrowser.Contracts;

using RolePlayer.UI.EmoteBrowser.Models;
using System.Collections.Generic;

public interface IEmoteRepository {
    IEnumerable<EmoteDisplayData> GetBaseEmotes();
}