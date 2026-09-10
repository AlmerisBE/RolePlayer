namespace RolePlayer.Tests.UI.GameDashboard.Presenters;

using NSubstitute;
using RolePlayer.Core.GameEngine.Contracts;
using RolePlayer.Core.GameEngine.Models;
using RolePlayer.UI.GameDashboard.Presenters;
using System.Collections.Generic;
using System.Linq;
using Xunit;

public class GameHostPresenterTests {
    [Fact]
    public void StartSession_PassesCorrectConfigurationToSessionService() {
        var mockLibraryService = Substitute.For<IGameLibraryService>();
        var mockSessionService = Substitute.For<IGameSessionService>();

        var testGame = new GameDefinition { Name = "Test Game" };
        mockLibraryService.GetAvailableGames().Returns(new List<GameDefinition> { testGame });

        using var presenter = new GameDashboardPresenter(mockLibraryService, mockSessionService);

        presenter.SelectGame(testGame);
        presenter.ToggleChannel(GameChatChannel.Say);
        presenter.ToggleChannel(GameChatChannel.Party);

        presenter.StartSession();

        mockSessionService.Received(1).StartSession(Arg.Is<GameSessionConfig>(config =>
            config.Game == testGame &&
            config.ListeningChannels.Count == 2 &&
            config.ListeningChannels.Contains(GameChatChannel.Say) &&
            config.ListeningChannels.Contains(GameChatChannel.Party)
        ));
    }

    [Fact]
    public void ToggleChannel_AddsChannelIfMissingAndRemovesIfPresent() {
        var mockLibraryService = Substitute.For<IGameLibraryService>();
        var mockSessionService = Substitute.For<IGameSessionService>();

        using var presenter = new GameDashboardPresenter(mockLibraryService, mockSessionService);

        presenter.ToggleChannel(GameChatChannel.Say);
        Assert.Contains(GameChatChannel.Say, presenter.SelectedChannels);

        presenter.ToggleChannel(GameChatChannel.Say);
        Assert.DoesNotContain(GameChatChannel.Say, presenter.SelectedChannels);
    }
}