namespace RolePlayer.Tests.Core.GameEngine.Services;

using NSubstitute;
using RolePlayer.Core.GameEngine.Contracts;
using RolePlayer.Core.GameEngine.Models;
using RolePlayer.Core.GameEngine.Services;
using RolePlayer.Core.Logging.Contracts;
using System.Collections.Generic;
using Xunit;

public class GameSessionServiceTests {
    private GameSessionService CreateService(out IGameEngine mockEngine) {
        var logger = Substitute.For<ILoggerService>();
        var factory = Substitute.For<IGameEngineFactory>();
        var watchers = new List<IGameEventWatcher>();
        var broadcaster = Substitute.For<IChatBroadcaster>();

        mockEngine = Substitute.For<IGameEngine>();
        factory.CreateEngine("StateMachineEngine").Returns(mockEngine);

        return new GameSessionService(logger, factory, watchers, broadcaster);
    }

    [Fact]
    public void StartSession_WithValidConfig_StartsEngineAndUpdatesState() {
        var service = this.CreateService(out var mockEngine);
        var config = new GameSessionConfig {
            Game = new GameDefinition { Name = "Test Game" },
            ListeningChannels = new HashSet<GameChatChannel> { GameChatChannel.Say }
        };

        bool result = service.StartSession(config);

        Assert.True(result);
        Assert.Equal(SessionState.WaitingForPlayers, service.CurrentState);
        mockEngine.Received(1).Start();
    }

    [Fact]
    public void StartSession_WithMissingChannels_ReturnsFalse() {
        var service = this.CreateService(out _);
        var config = new GameSessionConfig {
            Game = new GameDefinition { Name = "Test Game" }
        };

        bool result = service.StartSession(config);

        Assert.False(result);
        Assert.Equal(SessionState.Inactive, service.CurrentState);
    }

    [Fact]
    public void StopSession_WhenActive_StopsEngineAndUpdatesState() {
        var service = this.CreateService(out var mockEngine);
        var config = new GameSessionConfig {
            Game = new GameDefinition { Name = "Test Game" },
            ListeningChannels = new HashSet<GameChatChannel> { GameChatChannel.Say }
        };

        service.StartSession(config);
        service.StopSession();

        Assert.Equal(SessionState.Inactive, service.CurrentState);
        mockEngine.Received(1).Stop();
    }
}