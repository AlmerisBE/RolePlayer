namespace RolePlayer.API.GameEvents.Services;

using Dalamud.Game.Chat;
using Dalamud.Game.Text;
using Dalamud.Plugin.Services;
using RolePlayer.Core.GameEngine.Contracts;
using RolePlayer.Core.GameEngine.Models;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public class ChatWatcher : IGameEventWatcher {
    private IChatGui chatGui;
    private HashSet<string> participants = new(StringComparer.OrdinalIgnoreCase);
    private bool isWatching;

    public event Action<GameEvent>? EventFired;

    public ChatWatcher(IChatGui chatGui) {
        this.chatGui = chatGui;
    }

    public void Start() {
        if (this.isWatching) return;
        this.chatGui.ChatMessage += this.OnChatMessage;
        this.isWatching = true;
    }

    public void Stop() {
        if (!this.isWatching) return;
        this.chatGui.ChatMessage -= this.OnChatMessage;
        this.isWatching = false;
    }

    public void SetParticipants(IEnumerable<string> participantNames) {
        this.participants = new HashSet<string>(participantNames, StringComparer.OrdinalIgnoreCase);
    }

    public void ClearParticipants() {
        this.participants.Clear();
    }

    private void OnChatMessage(IChatMessage message) {
        if (message.Sender == null || message.Message == null) return;

        string senderName = message.Sender.TextValue;

        if (this.participants.Count > 0 && !this.participants.Contains(senderName)) return;

        string messageText = message.Message.TextValue;

        if ((int)message.LogKind == 73) {
            this.HandleDiceRoll(senderName, messageText);
            return;
        }

        var channel = this.MapChannel(message.LogKind);
        if (channel.HasValue) {
            this.EventFired?.Invoke(new ChatGameEvent {
                Sender = senderName,
                Message = messageText,
                Channel = channel.Value
            });
        }
    }

    private void HandleDiceRoll(string sender, string messageText) {
        var match = Regex.Match(messageText, @"(?:\D|^)(\d+)[^\d]+(\d+)(?:\D|$)");
        if (match.Success && match.Groups.Count >= 3) {
            if (int.TryParse(match.Groups[1].Value, out int roll) && int.TryParse(match.Groups[2].Value, out int outOf)) {
                this.EventFired?.Invoke(new DiceRollGameEvent {
                    Sender = sender,
                    Roll = roll,
                    OutOf = outOf
                });
            }
        }
    }

    private GameChatChannel? MapChannel(XivChatType type) {
        return type switch {
            XivChatType.Say => GameChatChannel.Say,
            XivChatType.Yell => GameChatChannel.Yell,
            XivChatType.Shout => GameChatChannel.Shout,
            XivChatType.Party => GameChatChannel.Party,
            XivChatType.Alliance => GameChatChannel.Alliance,
            XivChatType.FreeCompany => GameChatChannel.FreeCompany,
            _ => null
        };
    }

    public void Dispose() {
        this.Stop();
    }
}