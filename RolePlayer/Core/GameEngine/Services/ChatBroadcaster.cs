namespace RolePlayer.Core.GameEngine.Services;

using Dalamud.Plugin.Services;
using RolePlayer.API.Interop.Contracts;
using RolePlayer.Core.GameEngine.Contracts;
using RolePlayer.Core.GameEngine.Models;
using RolePlayer.Core.Logging.Contracts;
using System;
using System.Collections.Concurrent;

public class ChatBroadcaster : IChatBroadcaster, IDisposable {
    private INativeExecutionService nativeExecution;
    private IChatGui chatGui;
    private ILoggerService logger;
    private IFramework framework;

    private ConcurrentQueue<(string Message, GameChatChannel Channel)> messageQueue = new();
    private DateTime lastBroadcastTime = DateTime.MinValue;

    // FFXIV safely allows chat inputs spaced by ~1.2 to 1.5 seconds.
    private readonly TimeSpan broadcastDelay = TimeSpan.FromSeconds(1.5);

    public ChatBroadcaster(INativeExecutionService nativeExecution, IChatGui chatGui, ILoggerService logger, IFramework framework) {
        this.nativeExecution = nativeExecution;
        this.chatGui = chatGui;
        this.logger = logger;
        this.framework = framework;

        this.framework.Update += this.OnFrameworkUpdate;
    }

    public void Broadcast(string message, GameChatChannel channel) {
        if (string.IsNullOrWhiteSpace(message)) return;

        this.messageQueue.Enqueue((message, channel));
    }

    private void OnFrameworkUpdate(IFramework fw) {
        if (this.messageQueue.IsEmpty) return;

        if (DateTime.Now - this.lastBroadcastTime >= this.broadcastDelay) {
            if (this.messageQueue.TryDequeue(out var payload)) {
                this.ExecuteBroadcast(payload.Message, payload.Channel);
                this.lastBroadcastTime = DateTime.Now;
            }
        }
    }

    private void ExecuteBroadcast(string message, GameChatChannel channel) {
        string channelCmd = channel switch {
            GameChatChannel.Say => "/s",
            GameChatChannel.Yell => "/y",
            GameChatChannel.Shout => "/sh",
            GameChatChannel.Party => "/p",
            GameChatChannel.Alliance => "/a",
            GameChatChannel.FreeCompany => "/fc",
            _ => "/e"
        };

        string command = $"{channelCmd} {message}";

        try {
            this.nativeExecution.Execute(command);
        }
        catch (Exception ex) {
            this.logger.Error(ex, "[ChatBroadcaster] Native macro execution failed. Falling back to local echo.");
            this.chatGui.Print($"[Bot] {command}");
        }
    }

    public void Dispose() {
        this.framework.Update -= this.OnFrameworkUpdate;
    }
}