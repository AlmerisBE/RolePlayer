namespace RolePlayer.UI.Hotbar.Contracts;

using RolePlayer.UI.EmoteBrowser.Models;
using RolePlayer.UI.Hotbar.Models;
using System.Collections.Generic;

public interface IHotbarResolverService {
    List<EmoteDisplayData> ResolveEmotesForHotbar(HotbarConfig config, IEnumerable<EmoteDisplayData> allCachedEmotes);
}