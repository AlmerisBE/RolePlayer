namespace RolePlayer.Tests.UI.GameDashboard.Presenters;

using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Game.Text.SeStringHandling.Payloads;
using Dalamud.Plugin.Services;
using NSubstitute;
using RolePlayer.Core.Emotes.Contracts;
using RolePlayer.Core.GameEngine.Contracts;
using RolePlayer.Core.GameEngine.Models;
using RolePlayer.UI.GameDashboard.Presenters;
using Xunit;

public class GameHostPresenterTests {
    [Fact]
    public void SelectGame_WhenInactive_UpdatesSelectedGame() {
        var mockLibrary = Substitute.For<IGameLibraryService>();
        var mockSession = Substitute.For<IGameSessionService>();
        var mockTargetManager = Substitute.For<ITargetManager>();
        var mockEmoteCache = Substitute.For<IEmoteCache>();

        mockSession.CurrentState.Returns(SessionState.Inactive);

        using var presenter = new GameDashboardPresenter(mockLibrary, mockSession, mockTargetManager, mockEmoteCache);

        var game = new GameDefinition { Name = "Death Roll" };
        presenter.SelectGame(game);

        Assert.Equal(game, presenter.SelectedGame);
    }

    [Fact]
    public void ToggleChannel_WhenInactive_AddsOrRemovesChannel() {
        var mockLibrary = Substitute.For<IGameLibraryService>();
        var mockSession = Substitute.For<IGameSessionService>();
        var mockTargetManager = Substitute.For<ITargetManager>();
        var mockEmoteCache = Substitute.For<IEmoteCache>();

        mockSession.CurrentState.Returns(SessionState.Inactive);

        using var presenter = new GameDashboardPresenter(mockLibrary, mockSession, mockTargetManager, mockEmoteCache);

        presenter.ToggleChannel(GameChatChannel.Say);
        Assert.Contains(GameChatChannel.Say, presenter.SelectedChannels);

        presenter.ToggleChannel(GameChatChannel.Say);
        Assert.DoesNotContain(GameChatChannel.Say, presenter.SelectedChannels);
    }

    [Fact]
    public void AddTarget_WithValidTarget_CallsSessionServiceAddParticipant() {
        var mockLibrary = Substitute.For<IGameLibraryService>();
        var mockSession = Substitute.For<IGameSessionService>();
        var mockTargetManager = Substitute.For<ITargetManager>();
        var mockEmoteCache = Substitute.For<IEmoteCache>();

        var mockGameObject = Substitute.For<IGameObject>();
        mockGameObject.Name.Returns(new SeString(new TextPayload("Jane Doe")));
        mockTargetManager.Target.Returns(mockGameObject);

        using var presenter = new GameDashboardPresenter(mockLibrary, mockSession, mockTargetManager, mockEmoteCache);

        presenter.AddTarget();

        mockSession.Received(1).AddParticipant("Jane Doe");
    }
}