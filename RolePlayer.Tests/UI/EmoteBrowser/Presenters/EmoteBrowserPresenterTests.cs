namespace RolePlayer.Tests.UI.EmoteBrowser.Presenters;

using NSubstitute;
using RolePlayer.Core.Configuration.Contracts;
using RolePlayer.Core.Configuration.Models;
using RolePlayer.Core.Emotes.Contracts;
using RolePlayer.Core.Emotes.Models;
using RolePlayer.UI.EmoteBrowser.Contracts;
using RolePlayer.UI.EmoteBrowser.Presenters;
using RolePlayer.UI.Localization.Contracts;
using System.Collections.Generic;
using Xunit;

public class EmoteBrowserPresenterTests {
    private IEmoteCache emoteCache;
    private IContextManagementService contextService;
    private IGroupManagementService groupService;
    private ITagManagementService tagService;
    private ILocalizationService localization;

    public EmoteBrowserPresenterTests() {
        this.emoteCache = Substitute.For<IEmoteCache>();
        this.contextService = Substitute.For<IContextManagementService>();
        this.groupService = Substitute.For<IGroupManagementService>();
        this.tagService = Substitute.For<ITagManagementService>();
        this.localization = Substitute.For<ILocalizationService>();

        this.contextService.GetCurrentContext().Returns(new EmoteContext());

        this.localization.Translate(Arg.Any<string>()).Returns(x => x.Arg<string>());
    }

    private EmoteBrowserPresenter CreatePresenter() {
        return new EmoteBrowserPresenter(
            this.emoteCache,
            this.contextService,
            this.groupService,
            this.tagService,
            this.localization
        );
    }

    [Fact]
    public void ApplyFilters_SortsEmotesByName_WhenSortColumnIsSet() {
        // Arrange
        var presenter = this.CreatePresenter();

        var emotes = new List<EnrichedEmote> {
            new EnrichedEmote { Id = 1, Name = "Zeta", Category = "General", IsUnlocked = true },
            new EnrichedEmote { Id = 2, Name = "Alpha", Category = "General", IsUnlocked = true },
            new EnrichedEmote { Id = 3, Name = "Beta", Category = "General", IsUnlocked = true }
        };

        this.emoteCache.IsReady.Returns(true);
        this.emoteCache.GetCachedEmotes().Returns(emotes);

        // Act
        presenter.Refresh();
        presenter.SetSort(1, false);

        // Assert
        var grouped = presenter.GroupedEmotes;

        Assert.True(grouped.ContainsKey("General"));
        var list = grouped["General"];

        Assert.Equal(3, list.Count);
        Assert.Equal("Alpha", list[0].Name);
        Assert.Equal("Beta", list[1].Name);
        Assert.Equal("Zeta", list[2].Name);
    }
}