namespace RolePlayer.Core.Emotes.Contracts;

using RolePlayer.Core.Emotes.Models;
using System.Collections.Generic;

public interface IRawEmoteRepository {
    IEnumerable<EnrichedEmote> GetBaseEmotes();
}