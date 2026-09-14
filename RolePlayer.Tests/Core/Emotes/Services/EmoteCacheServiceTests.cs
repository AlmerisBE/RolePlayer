namespace RolePlayer.Tests.Core.Emotes.Services;

using NSubstitute;
using RolePlayer.Core.Emotes.Contracts;
using RolePlayer.Core.Emotes.Models;
using RolePlayer.Core.Emotes.Services;
using RolePlayer.Core.Logging.Contracts;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

public class EmoteCacheServiceTests {
    [Fact]
    public async Task ForceRefresh_UpdatesCacheAndFiresEvent() {
        var mockRepo = Substitute.For<IRawEmoteRepository>();
        var mockPlayerState = Substitute.For<IPlayerUnlockState>();
        var mockModState = Substitute.For<IEmoteModState>();
        var mockLogger = Substitute.For<ILoggerService>();

        mockPlayerState.IsPlayerValid.Returns(true);
        mockPlayerState.IsEmoteUnlocked(Arg.Any<uint>()).Returns(true);
        mockModState.GetModNameModifyingEmote(Arg.Any<uint>()).Returns(string.Empty);

        mockRepo.GetBaseEmotes().Returns(new List<EnrichedEmote> {
            new EnrichedEmote { Id = 1, Name = "Test Emote" }
        });

        bool eventFired = false;

        using var service = new EmoteCacheService(mockRepo, mockPlayerState, mockModState, mockLogger);
        service.CacheUpdated += () => eventFired = true;

        await Task.Delay(100);

        Assert.True(service.IsReady);
        Assert.True(eventFired);
        Assert.Single(service.GetCachedEmotes());
        Assert.Equal("Test Emote", service.GetCachedEmotes()[0].Name);
    }
}