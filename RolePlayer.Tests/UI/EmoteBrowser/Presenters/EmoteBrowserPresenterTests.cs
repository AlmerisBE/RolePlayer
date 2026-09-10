namespace RolePlayer.Tests.UI.EmoteBrowser.Presenters;

using NSubstitute;
using RolePlayer.Core.Configuration.Contracts;
using RolePlayer.Core.Configuration.Models;
using RolePlayer.Core.Logging.Contracts;
using RolePlayer.UI.EmoteBrowser.Contracts;
using RolePlayer.UI.EmoteBrowser.Models;
using RolePlayer.UI.EmoteBrowser.Presenters;
using RolePlayer.UI.Localization.Contracts;
using System.Collections.Generic;
using Xunit;

public class EmoteBrowserPresenterTests {
    [Fact]
    public void ApplyFilters_SortsEmotesByName_WhenSortColumnIsSet() {
        var mockEmoteRepo = Substitute.For<IEmoteRepository>();
        var mockPlayerState = Substitute.For<IPlayerStateProvider>();
        var mockModState = Substitute.For<IModStateProvider>();
        var mockContextService = Substitute.For<IContextManagementService>();
        var mockGroupService = Substitute.For<IGroupManagementService>();
        var mockTagService = Substitute.For<ITagManagementService>();
        var mockLocalization = Substitute.For<ILocalizationService>();
        var mockLogger = Substitute.For<ILoggerService>();

        mockPlayerState.IsPlayerValid.Returns(true);
        mockLocalization.Translate(Arg.Any<string>()).Returns("All");
        mockContextService.GetCurrentContext().Returns(new EmoteContext { CurrentGrouping = GroupingMode.None });

        var emotes = new List<EmoteDisplayData> {
            new EmoteDisplayData { Id = 1, Name = "Zebra" },
            new EmoteDisplayData { Id = 2, Name = "Apple" }
        };
        mockEmoteRepo.GetBaseEmotes().Returns(emotes);

        var presenter = new EmoteBrowserPresenter(
            mockEmoteRepo, mockPlayerState, mockModState, mockContextService,
            mockGroupService, mockTagService, mockLocalization, mockLogger);

        // Déclenche le chargement asynchrone synchrone pour les tests en forçant le cache (Simulé via Reflection ou en l'appelant)
        // Dans une approche stricte, on utiliserait un test asynchrone, mais on peut vérifier ApplyFilters directement
        var cacheField = typeof(EmoteBrowserPresenter).GetField("emotesCache", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        cacheField!.SetValue(presenter, emotes);

        presenter.SetSort(1, false);

        Assert.True(presenter.GroupedEmotes.ContainsKey("All"));
        var list = presenter.GroupedEmotes["All"];
        Assert.Equal("Apple", list[0].Name);
        Assert.Equal("Zebra", list[1].Name);
    }
}