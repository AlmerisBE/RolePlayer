namespace RolePlayer.Tests.API.GameEvents.Services;

using Dalamud.Game.Chat;
using Dalamud.Game.Text;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Game.Text.SeStringHandling.Payloads;
using Dalamud.Plugin.Services;
using NSubstitute;
using RolePlayer.API.GameEvents.Services;
using RolePlayer.Core.GameEngine.Models;
using System.Collections.Generic;
using Xunit;

public class ChatWatcherTests {
    [Fact]
    public void EventFired_WhenChatReceived_AndSenderIsParticipant_FiresChatEvent() {
        var mockChatGui = Substitute.For<IChatGui>();
        var mockObjectTable = Substitute.For<IObjectTable>();
        using var watcher = new ChatWatcher(mockChatGui, mockObjectTable);

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
        using var watcher = new ChatWatcher(mockChatGui, mockObjectTable);
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
        Assert.Equal(100, ((DiceRollGameEvent)capturedEvent).OutOf);
    }

    [Fact]
    public void EventFired_WhenSenderIsNotParticipant_DoesNotFire() {
        var mockChatGui = Substitute.For<IChatGui>();
        var mockObjectTable = Substitute.For<IObjectTable>();
        using var watcher = new ChatWatcher(mockChatGui, mockObjectTable);

        watcher.SetParticipants(new List<string> { "John Doe" });
        watcher.Start();

        bool fired = false;
        watcher.EventFired += e => fired = true;

        var mockMessage = Substitute.For<IChatMessage>();
        mockMessage.Sender.Returns(new SeString(new TextPayload("Jane Doe")));
        mockMessage.Message.Returns(new SeString(new TextPayload("Hello")));
        mockMessage.LogKind.Returns(XivChatType.Say);

        var method = typeof(ChatWatcher).GetMethod("OnChatMessage", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        method!.Invoke(watcher, new object[] { mockMessage });

        Assert.False(fired);
    }
}