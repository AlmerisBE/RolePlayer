namespace RolePlayer.UI.GameDashboard.Contracts;

using RolePlayer.Core.Emotes.Models;
using System.Collections.Generic;

public interface IEmoteCacheProvider {
    IReadOnlyList<EnrichedEmote> GetEmoteCache();
}