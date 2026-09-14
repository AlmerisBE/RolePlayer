namespace RolePlayer.Core.Emotes.Contracts;

using RolePlayer.Core.Emotes.Models;
using System;
using System.Collections.Generic;

public interface IEmoteCache {
    event Action? CacheUpdated;
    bool IsReady { get; }
    IReadOnlyList<EnrichedEmote> GetCachedEmotes();
}