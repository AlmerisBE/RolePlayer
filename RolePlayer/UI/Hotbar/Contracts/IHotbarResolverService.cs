namespace RolePlayer.UI.Hotbar.Contracts;

using RolePlayer.Core.Emotes.Models;
using RolePlayer.UI.Hotbar.Models;
using System.Collections.Generic;

public interface IHotbarResolverService {
    List<ResolvedHotbarItem> ResolveItemsForHotbar(HotbarConfig config, IEnumerable<EnrichedEmote> allCachedEmotes);
}