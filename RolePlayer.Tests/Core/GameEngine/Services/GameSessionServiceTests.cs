namespace RolePlayer.Tests.Core.GameEngine.Services;

using NSubstitute;
using RolePlayer.Core.GameEngine.Contracts;
using RolePlayer.Core.GameEngine.Models;
using RolePlayer.Core.GameEngine.Services;
using RolePlayer.Core.Logging.Contracts;
using System.Collections.Generic;
using Xunit;

public class GameSessionServiceTests {
    [Fact]
    public void StartSession_WithValidConfig_ChangesStateToWaitingForPlayers() {
        var mockLogger = Substitute.For<ILoggerService>();
        var mockFactory = Substitute.For<IGameEngineFactory>();
        var mockEngine = Substitute.For<IGameEngine>();
        var mockWatchers = new List<IGameEventWatcher>();
        var mockBroadcaster = Substitute.For<IChatBroadcaster>();

        mockFactory.CreateEngine(Arg.Any<string>()).Returns(mockEngine);

        var service = new GameSessionService(mockLogger, mockFactory, mockWatchers, mockBroadcaster);

        var config = new GameSessionConfig {
            Game = new GameDefinition { Name = "Test Game", EngineType = "TestEngine" }
        };
        config.ListeningChannels.Add(GameChatChannel.Say);

        bool eventTriggered = false;
        service.SessionStateChanged += () => eventTriggered = true;

        bool result = service.StartSession(config);

        Assert.True(result);
        Assert.Equal(SessionState.WaitingForPlayers, service.CurrentState);
        Assert.NotNull(service.CurrentConfig);
        Assert.True(eventTriggered);
        mockEngine.Received(1).Initialize(config);
        mockEngine.Received(1).Start();
    }

    [Fact]
    public void StartSession_WithNoGameDefinition_ReturnsFalseAndStaysInactive() {
        var mockLogger = Substitute.For<ILoggerService>();
        var mockFactory = Substitute.For<IGameEngineFactory>();
        var mockWatchers = new List<IGameEventWatcher>();
        var mockBroadcaster = Substitute.For<IChatBroadcaster>();

        var service = new GameSessionService(mockLogger, mockFactory, mockWatchers, mockBroadcaster);

        var config = new GameSessionConfig {
            Game = null
        };
        config.ListeningChannels.Add(GameChatChannel.Say);

        bool result = service.StartSession(config);

        Assert.False(result);
        Assert.Equal(SessionState.Inactive, service.CurrentState);
    }

    [Fact]
    public void StartSession_WithNoListeningChannels_ReturnsFalseAndStaysInactive() {
        var mockLogger = Substitute.For<ILoggerService>();
        var mockFactory = Substitute.For<IGameEngineFactory>();
        var mockWatchers = new List<IGameEventWatcher>();
        var mockBroadcaster = Substitute.For<IChatBroadcaster>();

        var service = new GameSessionService(mockLogger, mockFactory, mockWatchers, mockBroadcaster);

        var config = new GameSessionConfig {
            Game = new GameDefinition { Name = "Test Game", EngineType = "TestEngine" }
        };

        bool result = service.StartSession(config);

        Assert.False(result);
        Assert.Equal(SessionState.Inactive, service.CurrentState);
    }

    [Fact]
    public void StopSession_WhenActive_ResetsStateToInactiveAndClearsConfig() {
        var mockLogger = Substitute.For<ILoggerService>();
        var mockFactory = Substitute.For<IGameEngineFactory>();
        var mockEngine = Substitute.For<IGameEngine>();
        var mockWatchers = new List<IGameEventWatcher>();
        var mockBroadcaster = Substitute.For<IChatBroadcaster>();

        mockFactory.CreateEngine(Arg.Any<string>()).Returns(mockEngine);

        var service = new GameSessionService(mockLogger, mockFactory, mockWatchers, mockBroadcaster);

        var config = new GameSessionConfig {
            Game = new GameDefinition { Name = "Test Game", EngineType = "TestEngine" }
        };
        config.ListeningChannels.Add(GameChatChannel.Party);
        service.StartSession(config);

        bool eventTriggered = false;
        service.SessionStateChanged += () => eventTriggered = true;

        service.StopSession();

        Assert.Equal(SessionState.Inactive, service.CurrentState);
        Assert.Null(service.CurrentConfig);
        Assert.True(eventTriggered);
        mockEngine.Received(1).Stop();
    }
}