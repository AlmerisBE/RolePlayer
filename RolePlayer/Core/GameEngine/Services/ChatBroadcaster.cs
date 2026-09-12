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

    private DateTime lastBroadcastTime = DateTime.Now;

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

        var formattedMessage = message.Replace("\\n", "\n");
        var lines = formattedMessage.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries);
        var sb = new System.Text.StringBuilder();

        foreach (var line in lines) {
            var trimmed = line.Trim();
            if (trimmed.StartsWith("/") || trimmed.StartsWith("<")) sb.AppendLine(trimmed);
            else sb.AppendLine($"{channelCmd} {trimmed}");
        }

        string finalCommand = sb.ToString().TrimEnd('\r', '\n');

        try {
            this.nativeExecution.Execute(finalCommand);
        }
        catch (Exception ex) {
            this.logger.Error(ex, "[ChatBroadcaster] Native macro execution failed. Falling back to local echo.");
            this.chatGui.Print($"[Bot] {finalCommand}");
        }
    }

    public void Dispose() {
        this.framework.Update -= this.OnFrameworkUpdate;
    }
}