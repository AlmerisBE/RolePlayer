namespace RolePlayer.API.GameEvents.Services;

using Dalamud.Game.Chat;
using Dalamud.Game.Text;
using Dalamud.Game.Text.SeStringHandling.Payloads;
using Dalamud.Plugin.Services;
using RolePlayer.Core.GameEngine.Contracts;
using RolePlayer.Core.GameEngine.Models;
using System;
using System.Collections.Generic;
using System.Linq;
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
        string messageText = message.Message.TextValue;

        // 1. Détection hybride (Canal officiel OU Mots-clés infaillibles de jets de dés)
        bool isDiceRoll = false;
        try {
            isDiceRoll = (int)message.LogKind == 73;
        }
        catch { }

        var textLower = messageText.ToLowerInvariant();
        if (!isDiceRoll && (textLower.Contains("you roll a") || textLower.Contains("vous jetez") || textLower.Contains("du würfelst") || textLower.Contains("ダイスを振り"))) {
            isDiceRoll = true;
        }

        if (isDiceRoll) {
            this.HandleDiceRoll(message, messageText);
            return;
        }

        // 2. Traitement du chat standard
        string senderName = this.CleanPlayerName(message.Sender?.TextValue ?? string.Empty);
        if (string.IsNullOrEmpty(senderName)) return;

        if (this.RestrictToParticipants && this.participants.Count > 0 && !this.participants.Contains(senderName)) return;

        var channel = this.MapChannel(message.LogKind);
        if (channel.HasValue) {
            this.EventFired?.Invoke(new ChatGameEvent {
                Sender = senderName,
                Message = messageText,
                Channel = channel.Value
            });
        }
    }

    private void HandleDiceRoll(IChatMessage message, string messageText) {
        // Extraction brutale de tous les nombres du message pour ignorer le formatage/icônes
        var numbers = Regex.Matches(messageText, @"\d+").Cast<Match>().Select(m => int.Parse(m.Value)).ToList();

        if (numbers.Count >= 2) {
            // Dans 99% des cas (FR/EN/DE), le jet et le max sont les deux derniers nombres
            int roll = numbers[numbers.Count - 2];
            int outOf = numbers[numbers.Count - 1];

            // Inversion spécifique pour la localisation Japonaise
            if (messageText.Contains("から") || messageText.Contains("出")) {
                outOf = numbers[numbers.Count - 2];
                roll = numbers[numbers.Count - 1];
            }

            string sender = this.CleanPlayerName(message.Sender?.TextValue ?? string.Empty);

            // Repli 1 : Payload Natif
            if (string.IsNullOrEmpty(sender)) {
                foreach (var payload in message.Message.Payloads) {
                    if (payload is PlayerPayload pp) {
                        sender = this.CleanPlayerName(pp.PlayerName);
                        break;
                    }
                }
            }

            // Repli 2 : Participant connu explicitement cité (Joueurs distants)
            if (string.IsNullOrEmpty(sender)) {
                foreach (var p in this.participants) {
                    if (messageText.Contains(p, StringComparison.OrdinalIgnoreCase)) {
                        sender = p;
                        break;
                    }
                }
            }

            // Repli 3 : Pronoms système (Joueur local)
            if (string.IsNullOrEmpty(sender)) {
                var textLower = messageText.ToLowerInvariant();
                if (textLower.Contains("you roll") || textLower.Contains("vous jetez") || textLower.Contains("vous obtenez") || textLower.Contains("du würfelst") || textLower.Contains("を出した")) {
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