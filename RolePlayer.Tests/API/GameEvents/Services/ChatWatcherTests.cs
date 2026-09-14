namespace RolePlayer.Tests.API.GameEvents.Services;

using Dalamud.Game.Chat;
using Dalamud.Game.Text;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Game.Text.SeStringHandling.Payloads;
using Dalamud.Plugin.Services;
using NSubstitute;
using RolePlayer.API.GameEvents.Contracts;
using RolePlayer.API.GameEvents.Services;
using RolePlayer.Core.GameEngine.Models;
using RolePlayer.Core.Logging.Contracts;
using System.Collections.Generic;
using Xunit;

public class ChatWatcherTests {
    [Fact]
    public void EventFired_WhenChatReceived_AndSenderIsParticipant_FiresChatEvent() {
        var mockChatGui = Substitute.For<IChatGui>();
        var mockObjectTable = Substitute.For<IObjectTable>();
        var mockLogger = Substitute.For<ILoggerService>();
        var mockDiceParser = Substitute.For<IDiceRollParser>();
        var mockNameNormalizer = Substitute.For<IPlayerNameNormalizer>();

        mockNameNormalizer.Normalize(Arg.Any<string>()).Returns(callInfo => callInfo.Arg<string>());

        using var watcher = new ChatWatcher(mockChatGui, mockObjectTable, mockLogger, mockDiceParser, mockNameNormalizer);

        watcher.SetParticipants(new List<string> { "John Doe" });
        watcher.Start();

        GameEvent? capturedEvent = null;
        watcher.EventFired += e => capturedEvent = e;

        var mockMessage = Substitute.For<IChatMessage>();
        mockMessage.Sender.Returns(new SeString(new TextPayload("John Doe")));
        mockMessage.Message.Returns(new SeString(new TextPayload("Hello")));
        mockMessage.LogKind.Returns(XivChatType.Say);

        var method = typeof(ChatWatcher).GetMethod("OnChatMessage", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        method!.Invoke(watcher, new object[] { mockMessage });

        Assert.NotNull(capturedEvent);
        Assert.IsType<ChatGameEvent>(capturedEvent);
        Assert.Equal("John Doe", capturedEvent.Sender);
        Assert.Equal("Hello", ((ChatGameEvent)capturedEvent).Message);
        Assert.Equal(GameChatChannel.Say, ((ChatGameEvent)capturedEvent).Channel);
    }

    [Fact]
    public void EventFired_WhenDiceRolled_ParsesValuesCorrectly() {
        var mockChatGui = Substitute.For<IChatGui>();
        var mockObjectTable = Substitute.For<IObjectTable>();
        var mockLogger = Substitute.For<ILoggerService>();
        var mockDiceParser = Substitute.For<IDiceRollParser>();
        var mockNameNormalizer = Substitute.For<IPlayerNameNormalizer>();

        mockNameNormalizer.Normalize(Arg.Any<string>()).Returns(callInfo => callInfo.Arg<string>());

        mockDiceParser.TryParse(Arg.Any<string>(), out Arg.Any<int>(), out Arg.Any<int>())
            .Returns(x => {
                x[1] = 42;
                x[2] = 100;
                return true;
            });

        using var watcher = new ChatWatcher(mockChatGui, mockObjectTable, mockLogger, mockDiceParser, mockNameNormalizer);
        watcher.Start();

        GameEvent? capturedEvent = null;
        watcher.EventFired += e => capturedEvent = e;

        var mockMessage = Substitute.For<IChatMessage>();
        mockMessage.Sender.Returns(new SeString(new TextPayload("Jane Doe")));
        mockMessage.Message.Returns(new SeString(new TextPayload("Random! You roll a 42 (out of 100).")));
        mockMessage.LogKind.Returns((XivChatType)73);

        var method = typeof(ChatWatcher).GetMethod("OnChatMessage", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        method!.Invoke(watcher, new object[] { mockMessage });

        Assert.NotNull(capturedEvent);
        Assert.IsType<DiceRollGameEvent>(capturedEvent);
        Assert.Equal(42, ((DiceRollGameEvent)capturedEvent).Roll);
        Assert.Equal(100, ((DiceRollGameEvent)capturedEvent).MaxRoll);
    }
}