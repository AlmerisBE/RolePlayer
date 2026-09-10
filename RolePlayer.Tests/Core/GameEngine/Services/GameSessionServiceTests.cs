namespace RolePlayer.Tests.Core.GameEngine.Services;

using NSubstitute;
using RolePlayer.Core.GameEngine.Models;
using RolePlayer.Core.GameEngine.Services;
using RolePlayer.Core.Logging.Contracts;
using Xunit;

public class GameSessionServiceTests {
    [Fact]
    public void StartSession_WithValidConfig_ChangesStateToWaitingForPlayers() {
        var mockLogger = Substitute.For<ILoggerService>();
        var service = new GameSessionService(mockLogger);

        var config = new GameSessionConfig {
            Game = new GameDefinition { Name = "Test Game" }
        };
        config.ListeningChannels.Add(GameChatChannel.Say);

        bool eventTriggered = false;
        service.SessionStateChanged += () => eventTriggered = true;

        bool result = service.StartSession(config);

        Assert.True(result);
        Assert.Equal(SessionState.WaitingForPlayers, service.CurrentState);
        Assert.NotNull(service.CurrentConfig);
        Assert.True(eventTriggered);
    }

    [Fact]
    public void StartSession_WithNoGameDefinition_ReturnsFalseAndStaysInactive() {
        var mockLogger = Substitute.For<ILoggerService>();
        var service = new GameSessionService(mockLogger);

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
        var service = new GameSessionService(mockLogger);

        var config = new GameSessionConfig {
            Game = new GameDefinition { Name = "Test Game" }
        };

        bool result = service.StartSession(config);

        Assert.False(result);
        Assert.Equal(SessionState.Inactive, service.CurrentState);
    }

    [Fact]
    public void StopSession_WhenActive_ResetsStateToInactiveAndClearsConfig() {
        var mockLogger = Substitute.For<ILoggerService>();
        var service = new GameSessionService(mockLogger);

        var config = new GameSessionConfig { Game = new GameDefinition { Name = "Test Game" } };
        config.ListeningChannels.Add(GameChatChannel.Party);
        service.StartSession(config);

        bool eventTriggered = false;
        service.SessionStateChanged += () => eventTriggered = true;

        service.StopSession();

        Assert.Equal(SessionState.Inactive, service.CurrentState);
        Assert.Null(service.CurrentConfig);
        Assert.True(eventTriggered);
    }
}