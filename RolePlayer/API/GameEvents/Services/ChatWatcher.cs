namespace RolePlayer.API.GameEvents.Services;

using Dalamud.Game.Chat;
using Dalamud.Game.Text;
using Dalamud.Game.Text.SeStringHandling.Payloads;
using Dalamud.Plugin.Services;
using RolePlayer.Core.GameEngine.Contracts;
using RolePlayer.Core.GameEngine.Models;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public class ChatWatcher : IGameEventWatcher {
    private IChatGui chatGui;
    private IObjectTable objectTable;
    private HashSet<string> participants = new(StringComparer.OrdinalIgnoreCase);
    private bool isWatching;

    public event Action<GameEvent>? EventFired;
    public bool RestrictToParticipants { get; set; } = false;

    public ChatWatcher(IChatGui chatGui, IObjectTable objectTable) {
        this.chatGui = chatGui;
        this.objectTable = objectTable;
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

    private string CleanPlayerName(string name) {
        if (string.IsNullOrEmpty(name)) return string.Empty;
        var parts = name.Split(new[] { '\uE05D', '@' }, 2);
        return parts[0].Trim();
    }

    private void OnChatMessage(IChatMessage message) {
        if (message.Message == null) return;

        if ((int)message.LogKind == 73) {
            this.HandleDiceRoll(message);
            return;
        }

        string senderName = this.CleanPlayerName(message.Sender?.TextValue ?? string.Empty);
        if (string.IsNullOrEmpty(senderName)) return;

        if (this.RestrictToParticipants && this.participants.Count > 0 && !this.participants.Contains(senderName)) return;

        string messageText = message.Message.TextValue;

        var channel = this.MapChannel(message.LogKind);
        if (channel.HasValue) {
            this.EventFired?.Invoke(new ChatGameEvent {
                Sender = senderName,
                Message = messageText,
                Channel = channel.Value
            });
        }
    }

    private void HandleDiceRoll(IChatMessage message) {
        string messageText = message.Message.TextValue;
        var match = Regex.Match(messageText, @"(?:\D|^)(\d+)[^\d]+(\d+)(?:\D|$)");

        if (match.Success && match.Groups.Count >= 3) {
            if (int.TryParse(match.Groups[1].Value, out int roll) && int.TryParse(match.Groups[2].Value, out int outOf)) {

                // 1. Tenter d'utiliser directement l'expéditeur natif du message
                string sender = this.CleanPlayerName(message.Sender?.TextValue ?? string.Empty);

                // 2. Repli 1 : chercher le nom exact via le Payload natif de FFXIV
                if (string.IsNullOrEmpty(sender)) {
                    foreach (var payload in message.Message.Payloads) {
                        if (payload is PlayerPayload pp) {
                            sender = this.CleanPlayerName(pp.PlayerName);
                            break;
                        }
                    }
                }

                // 3. Repli 2 : chercher un participant existant dans le texte
                if (string.IsNullOrEmpty(sender)) {
                    foreach (var p in this.participants) {
                        if (messageText.Contains(p, StringComparison.OrdinalIgnoreCase)) {
                            sender = p;
                            break;
                        }
                    }
                }

                // 4. Repli ultime : gérer les textes système traduits ("Vous", "You", "Du") 
                if (string.IsNullOrEmpty(sender)) {
                    var textLower = messageText.ToLowerInvariant();
                    if (textLower.Contains("you roll") ||
                        textLower.Contains("vous jetez") ||
                        textLower.Contains("vous obtenez") ||
                        textLower.Contains("du würfelst") ||
                        textLower.Contains("を出した")) {

                        var localPlayer = this.objectTable.LocalPlayer;
                        if (localPlayer != null) {
                            sender = this.CleanPlayerName(localPlayer.Name.TextValue);
                        }
                    }
                }

                if (!string.IsNullOrEmpty(sender)) {
                    this.EventFired?.Invoke(new DiceRollGameEvent {
                        Sender = sender,
                        Roll = roll,
                        OutOf = outOf
                    });
                }
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