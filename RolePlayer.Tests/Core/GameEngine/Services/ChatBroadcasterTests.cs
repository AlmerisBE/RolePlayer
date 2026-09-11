namespace RolePlayer.Tests.Core.GameEngine.Services;

using Dalamud.Plugin.Services;
using NSubstitute;
using RolePlayer.API.Interop.Contracts;
using RolePlayer.Core.GameEngine.Models;
using RolePlayer.Core.GameEngine.Services;
using RolePlayer.Core.Logging.Contracts;
using System;
using System.Reflection;
using Xunit;

public class ChatBroadcasterTests {
    [Fact]
    public void Broadcast_EnqueuesMessage_DoesNotExecuteImmediately() {
        var mockExecution = Substitute.For<INativeExecutionService>();
        var mockChatGui = Substitute.For<IChatGui>();
        var mockLogger = Substitute.For<ILoggerService>();
        var mockFramework = Substitute.For<IFramework>();

        using var broadcaster = new ChatBroadcaster(mockExecution, mockChatGui, mockLogger, mockFramework);

        broadcaster.Broadcast("Hello World", GameChatChannel.Party);

        mockExecution.DidNotReceiveWithAnyArgs().Execute(default!);
    }

    [Fact]
    public void OnFrameworkUpdate_ExecutesQueuedMessage_RespectsInitialAndSubsequentDelays() {
        var mockExecution = Substitute.For<INativeExecutionService>();
        var mockChatGui = Substitute.For<IChatGui>();
        var mockLogger = Substitute.For<ILoggerService>();
        var mockFramework = Substitute.For<IFramework>();

        using var broadcaster = new ChatBroadcaster(mockExecution, mockChatGui, mockLogger, mockFramework);

        broadcaster.Broadcast("Message 1", GameChatChannel.Say);
        broadcaster.Broadcast("Message 2", GameChatChannel.Say);

        var updateMethod = typeof(ChatBroadcaster).GetMethod("OnFrameworkUpdate", BindingFlags.NonPublic | BindingFlags.Instance);
        var timeField = typeof(ChatBroadcaster).GetField("lastBroadcastTime", BindingFlags.NonPublic | BindingFlags.Instance);

        // 1. Initial tick: should NOT execute because of the forced initial delay
        updateMethod!.Invoke(broadcaster, new object[] { mockFramework });
        mockExecution.DidNotReceiveWithAnyArgs().Execute(default!);

        // Simulate elapsed time by shifting the internal timer back by 2 seconds
        timeField!.SetValue(broadcaster, DateTime.Now.AddSeconds(-2));

        // 2. Next tick after time elapsed: should execute the first message
        updateMethod.Invoke(broadcaster, new object[] { mockFramework });
        mockExecution.Received(1).Execute("/s Message 1");
        mockExecution.ClearReceivedCalls();

        // 3. Immediate subsequent tick: should NOT execute (throttled by the 1.5s delay)
        updateMethod.Invoke(broadcaster, new object[] { mockFramework });
        mockExecution.DidNotReceiveWithAnyArgs().Execute(default!);
    }
}