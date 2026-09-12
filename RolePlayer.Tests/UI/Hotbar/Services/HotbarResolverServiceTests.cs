namespace RolePlayer.Tests.UI.Hotbar.Services;

using NSubstitute;
using RolePlayer.Core.Configuration.Contracts;
using RolePlayer.Core.Emotes.Models;
using RolePlayer.Core.Macros.Models;
using RolePlayer.UI.EmoteBrowser.Contracts;
using RolePlayer.UI.Hotbar.Contracts;
using RolePlayer.UI.Hotbar.Models;
using RolePlayer.UI.Hotbar.Services;
using System.Collections.Generic;
using Xunit;

public class HotbarResolverServiceTests {
    private List<EnrichedEmote> GetDummyEmotes() {
        return new List<EnrichedEmote> {
            new EnrichedEmote { Id = 1, Name = "Sit", LocalizedCommand = "/sit", IsUnlocked = true, IconId = 100, Category = "General" },
            new EnrichedEmote { Id = 2, Name = "Dance", LocalizedCommand = "/dance", IsUnlocked = true, IconId = 101, Category = "Social" },
            new EnrichedEmote { Id = 3, Name = "LockedEmote", IsUnlocked = false, IconId = 102 },
            new EnrichedEmote { Id = 4, Name = "ModdedDance", LocalizedCommand = "/mdance", IsUnlocked = true, IconId = 103, IsModded = true, Category = "Social" }
        };
    }

    [Fact]
    public void ResolveItems_ManualMode_ReturnsOnlySpecifiedAndUnlockedEmotesWithIcons() {
        var mockGroupService = Substitute.For<IGroupManagementService>();
        var mockTagService = Substitute.For<ITagManagementService>();
        var mockMacroService = Substitute.For<IMacroManagementService>();
        var mockContextService = Substitute.For<IContextManagementService>();

        mockMacroService.GetMacros().Returns(new List<RoleplayMacro>());

        var service = new HotbarResolverService(mockGroupService, mockTagService, mockMacroService, mockContextService);

        var config = new HotbarConfig {
            PopulationMode = HotbarPopulationMode.Manual,
            ManualEmoteIds = new List<uint> { 1, 3 } // 3 is locked, should be filtered out
        };

        var result = service.ResolveItemsForHotbar(config, this.GetDummyEmotes());

        Assert.Single(result);
        Assert.Equal(1u, result[0].EmoteId);
    }

    [Fact]
    public void ResolveItems_DynamicMode_FiltersBySearchQuery() {
        var mockGroupService = Substitute.For<IGroupManagementService>();
        var mockTagService = Substitute.For<ITagManagementService>();
        var mockMacroService = Substitute.For<IMacroManagementService>();
        var mockContextService = Substitute.For<IContextManagementService>();

        mockMacroService.GetMacros().Returns(new List<RoleplayMacro>());

        var service = new HotbarResolverService(mockGroupService, mockTagService, mockMacroService, mockContextService);

        var config = new HotbarConfig {
            PopulationMode = HotbarPopulationMode.Dynamic,
            SearchQuery = "dance",
            TargetType = HotbarTargetType.Emotes
        };

        var result = service.ResolveItemsForHotbar(config, this.GetDummyEmotes());

        Assert.Equal(2, result.Count);
        Assert.Contains(result, e => e.EmoteId == 2);
        Assert.Contains(result, e => e.EmoteId == 4);
    }

    [Fact]
    public void ResolveItems_DynamicMode_FiltersByModdedOnly() {
        var mockGroupService = Substitute.For<IGroupManagementService>();
        var mockTagService = Substitute.For<ITagManagementService>();
        var mockMacroService = Substitute.For<IMacroManagementService>();
        var mockContextService = Substitute.For<IContextManagementService>();

        mockMacroService.GetMacros().Returns(new List<RoleplayMacro>());

        var service = new HotbarResolverService(mockGroupService, mockTagService, mockMacroService, mockContextService);

        var config = new HotbarConfig {
            PopulationMode = HotbarPopulationMode.Dynamic,
            ShowModdedOnly = true,
            TargetType = HotbarTargetType.Emotes
        };

        var result = service.ResolveItemsForHotbar(config, this.GetDummyEmotes());

        Assert.Single(result);
        Assert.Equal(4u, result[0].EmoteId);
    }
}