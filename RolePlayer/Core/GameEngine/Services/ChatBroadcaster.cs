namespace RolePlayer.Core.GameEngine.Services;

using Dalamud.Plugin.Services;
using RolePlayer.API.Interop.Contracts;
using RolePlayer.Core.GameEngine.Contracts;
using RolePlayer.Core.GameEngine.Models;
using RolePlayer.Core.Logging.Contracts;
using System;

public class ChatBroadcaster : IChatBroadcaster {
    private INativeExecutionService nativeExecution;
    private IChatGui chatGui;
    private ILoggerService logger;

    public ChatBroadcaster(INativeExecutionService nativeExecution, IChatGui chatGui, ILoggerService logger) {
        this.nativeExecution = nativeExecution;
        this.chatGui = chatGui;
        this.logger = logger;
    }

    public void Broadcast(string message, GameChatChannel channel) {
        if (string.IsNullOrWhiteSpace(message)) return;

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
}