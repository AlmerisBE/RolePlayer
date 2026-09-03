namespace RolePlayer.Tests.UI.Hotbar.Services;

using NSubstitute;
using RolePlayer.UI.EmoteBrowser.Contracts;
using RolePlayer.UI.EmoteBrowser.Models;
using RolePlayer.UI.Hotbar.Models;
using RolePlayer.UI.Hotbar.Services;
using System.Collections.Generic;
using Xunit;

public class HotbarResolverServiceTests {
    private List<EmoteDisplayData> GetDummyEmotes() {
        return new List<EmoteDisplayData> {
            new EmoteDisplayData { Id = 1, Name = "Sit", LocalizedCommand = "/sit", IsUnlocked = true, IconId = 100, Category = "General" },
            new EmoteDisplayData { Id = 2, Name = "Dance", LocalizedCommand = "/dance", IsUnlocked = true, IconId = 101, Category = "Social" },
            new EmoteDisplayData { Id = 3, Name = "LockedEmote", IsUnlocked = false, IconId = 102 },
            new EmoteDisplayData { Id = 4, Name = "ModdedDance", LocalizedCommand = "/mdance", IsUnlocked = true, IconId = 103, IsModded = true, Category = "Social" }
        };
    }

    [Fact]
    public void ResolveEmotes_ManualMode_ReturnsOnlySpecifiedAndUnlockedEmotesWithIcons() {
        var mockGroupService = Substitute.For<IGroupManagementService>();
        var mockTagService = Substitute.For<ITagManagementService>();
        var service = new HotbarResolverService(mockGroupService, mockTagService);

        var config = new HotbarConfig {
            PopulationMode = HotbarPopulationMode.Manual,
            ManualEmoteIds = new List<uint> { 1, 3 } // 3 is locked, should be filtered out
        };

        var result = service.ResolveEmotesForHotbar(config, this.GetDummyEmotes());

        Assert.Single(result);
        Assert.Equal(1u, result[0].Id);
    }

    [Fact]
    public void ResolveEmotes_DynamicMode_FiltersBySearchQuery() {
        var mockGroupService = Substitute.For<IGroupManagementService>();
        var mockTagService = Substitute.For<ITagManagementService>();
        var service = new HotbarResolverService(mockGroupService, mockTagService);

        var config = new HotbarConfig {
            PopulationMode = HotbarPopulationMode.Dynamic,
            SearchQuery = "dance"
        };

        var result = service.ResolveEmotesForHotbar(config, this.GetDummyEmotes());

        Assert.Equal(2, result.Count);
        Assert.Contains(result, e => e.Id == 2);
        Assert.Contains(result, e => e.Id == 4);
    }

    [Fact]
    public void ResolveEmotes_DynamicMode_FiltersByModdedOnly() {
        var mockGroupService = Substitute.For<IGroupManagementService>();
        var mockTagService = Substitute.For<ITagManagementService>();
        var service = new HotbarResolverService(mockGroupService, mockTagService);

        var config = new HotbarConfig {
            PopulationMode = HotbarPopulationMode.Dynamic,
            ShowModdedOnly = true
        };

        var result = service.ResolveEmotesForHotbar(config, this.GetDummyEmotes());

        Assert.Single(result);
        Assert.Equal(4u, result[0].Id);
    }
}