namespace RolePlayer.UI.Hotbar.Services;

using RolePlayer.Core.Configuration.Contracts;
using RolePlayer.Core.Emotes.Models;
using RolePlayer.UI.EmoteBrowser.Contracts;
using RolePlayer.UI.Hotbar.Contracts;
using RolePlayer.UI.Hotbar.Models;
using System.Collections.Generic;
using System.Linq;

public class HotbarResolverService : IHotbarResolverService {
    private IGroupManagementService groupManagementService;
    private ITagManagementService tagManagementService;
    private IMacroManagementService macroManagementService;
    private IContextManagementService contextManagementService;

    public HotbarResolverService(
        IGroupManagementService groupManagementService,
        ITagManagementService tagManagementService,
        IMacroManagementService macroManagementService,
        IContextManagementService contextManagementService) {

        this.groupManagementService = groupManagementService;
        this.tagManagementService = tagManagementService;
        this.macroManagementService = macroManagementService;
        this.contextManagementService = contextManagementService;
    }

    public List<ResolvedHotbarItem> ResolveItemsForHotbar(HotbarConfig config, IEnumerable<EnrichedEmote> allCachedEmotes) {
        var results = new List<ResolvedHotbarItem>();
        var validEmotes = allCachedEmotes.Where(e => e.IsUnlocked && e.IconId > 0).ToList();
        var allMacros = this.macroManagementService.GetMacros().ToList();

        if (config.PopulationMode == HotbarPopulationMode.Manual) {
            foreach (var e in validEmotes.Where(e => config.ManualEmoteIds.Contains(e.Id))) {
                results.Add(new ResolvedHotbarItem {
                    EmoteId = e.Id,
                    Name = e.Name,
                    IconId = e.IconId,
                    HasVariations = e.HasVariations,
                    IsModded = e.IsModded,
                    ModName = e.ModName,
                    CommandText = e.LocalizedCommand
                });
            }

            foreach (var m in allMacros.Where(m => config.ManualMacroIds.Contains(m.Id))) {
                results.Add(new ResolvedHotbarItem {
                    MacroId = m.Id,
                    MacroReference = m,
                    Name = m.Name,
                    IconId = m.IconId,
                    CommandText = "Macro"
                });
            }

            return results;
        }

        var query = config.SearchQuery.Trim().ToLowerInvariant();
        bool hasSearch = !string.IsNullOrEmpty(query);
        bool hasCatFilter = config.SelectedCategories.Count > 0;
        bool hasGroupFilter = config.SelectedGroups.Count > 0;
        bool hasTagFilter = config.SelectedTags.Count > 0;

        if (config.TargetType == HotbarTargetType.Emotes || config.TargetType == HotbarTargetType.Mixed) {
            foreach (var emote in validEmotes) {
                if (config.ShowModdedOnly && !emote.IsModded) continue;

                if (hasSearch) {
                    bool matchesName = emote.Name.ToLowerInvariant().Contains(query);
                    bool matchesCmd = emote.LocalizedCommand.ToLowerInvariant().Contains(query);
                    bool matchesEnCmd = !string.IsNullOrEmpty(emote.EnglishCommand) && emote.EnglishCommand.ToLowerInvariant().Contains(query);
                    if (!matchesName && !matchesCmd && !matchesEnCmd) continue;
                }

                if (hasCatFilter && !config.SelectedCategories.Contains(emote.Category)) continue;

                var customGroup = this.groupManagementService.GetGroupForEmote(emote.Id);
                if (hasGroupFilter && (string.IsNullOrEmpty(customGroup) || !config.SelectedGroups.Contains(customGroup))) continue;

                if (hasTagFilter) {
                    var tags = this.tagManagementService.GetTagsForEmote(emote.Id);
                    if (!config.SelectedTags.Overlaps(tags)) continue;
                }

                results.Add(new ResolvedHotbarItem {
                    EmoteId = emote.Id,
                    Name = emote.Name,
                    IconId = emote.IconId,
                    HasVariations = emote.HasVariations,
                    IsModded = emote.IsModded,
                    ModName = emote.ModName,
                    CommandText = emote.LocalizedCommand
                });
            }
        }

        if (config.TargetType == HotbarTargetType.Macros || config.TargetType == HotbarTargetType.Mixed) {
            foreach (var macro in allMacros) {
                var tags = this.tagManagementService.GetTagsForMacro(macro.Id);

                if (hasSearch) {
                    bool matchesName = macro.Name.ToLowerInvariant().Contains(query);
                    bool matchesTag = tags.Any(t => t.ToLowerInvariant().Contains(query));
                    if (!matchesName && !matchesTag) continue;
                }

                var customGroup = this.groupManagementService.GetGroupForMacro(macro.Id);
                if (hasGroupFilter && (string.IsNullOrEmpty(customGroup) || !config.SelectedGroups.Contains(customGroup))) continue;

                if (hasTagFilter && !config.SelectedTags.Overlaps(tags)) continue;

                results.Add(new ResolvedHotbarItem {
                    MacroId = macro.Id,
                    MacroReference = macro,
                    Name = macro.Name,
                    IconId = macro.IconId,
                    CommandText = "Macro"
                });
            }
        }

        return results;
    }
}