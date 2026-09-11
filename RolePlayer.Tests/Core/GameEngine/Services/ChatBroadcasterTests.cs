namespace RolePlayer.Tests.Core.GameEngine.Services;

using Dalamud.Plugin.Services;
using NSubstitute;
using RolePlayer.API.Interop.Contracts;
using RolePlayer.Core.GameEngine.Models;
using RolePlayer.Core.GameEngine.Services;
using RolePlayer.Core.Logging.Contracts;
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

        // Verification: The execute method should absolutely not be called synchronously
        mockExecution.DidNotReceiveWithAnyArgs().Execute(default!);
    }

    [Fact]
    public void OnFrameworkUpdate_ExecutesQueuedMessage_RespectsDelay() {
        var mockExecution = Substitute.For<INativeExecutionService>();
        var mockChatGui = Substitute.For<IChatGui>();
        var mockLogger = Substitute.For<ILoggerService>();
        var mockFramework = Substitute.For<IFramework>();

        using var broadcaster = new ChatBroadcaster(mockExecution, mockChatGui, mockLogger, mockFramework);

        broadcaster.Broadcast("Message 1", GameChatChannel.Say);
        broadcaster.Broadcast("Message 2", GameChatChannel.Say);

        var updateMethod = typeof(ChatBroadcaster).GetMethod("OnFrameworkUpdate", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        // Simulate first tick (bypasses initial MinValue delay)
        updateMethod!.Invoke(broadcaster, new object[] { mockFramework });

        mockExecution.Received(1).Execute("/s Message 1");
        mockExecution.ClearReceivedCalls();

        // Simulate immediate second tick (should be throttled by the 1.5s delay)
        updateMethod.Invoke(broadcaster, new object[] { mockFramework });

        mockExecution.DidNotReceiveWithAnyArgs().Execute(default!);
    }
}