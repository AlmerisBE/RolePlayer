namespace RolePlayer.API.GameEvents.Services;

using Dalamud.Game.Chat;
using Dalamud.Game.Text.SeStringHandling.Payloads;
using Dalamud.Plugin.Services;
using RolePlayer.API.GameEvents.Contracts;
using RolePlayer.API.GameEvents.Extensions;
using RolePlayer.Core.GameEngine.Contracts;
using RolePlayer.Core.GameEngine.Models;
using RolePlayer.Core.Logging.Contracts;
using System;
using System.Collections.Generic;

public class ChatWatcher : IGameEventWatcher {
    private IChatGui chatGui;
    private IObjectTable objectTable;
    private ILoggerService logger;
    private IDiceRollParser diceParser;
    private IPlayerNameNormalizer nameNormalizer;
    private HashSet<string> participants = new(StringComparer.OrdinalIgnoreCase);
    private bool isWatching;

    public event Action<GameEvent>? EventFired;
    public bool RestrictToParticipants { get; set; } = true;

    public ChatWatcher(
        IChatGui chatGui,
        IObjectTable objectTable,
        ILoggerService logger,
        IDiceRollParser diceParser,
        IPlayerNameNormalizer nameNormalizer) {

        this.chatGui = chatGui;
        this.objectTable = objectTable;
        this.logger = logger;
        this.diceParser = diceParser;
        this.nameNormalizer = nameNormalizer;
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

    private bool IsFallbackDiceRoll(string textLower) {
        return textLower.Contains("random!") ||
               textLower.Contains("you roll a") ||
               textLower.Contains("lancer d'un dé") || textLower.Contains("vous obtenez") || textLower.Contains("obtient un") || textLower.Contains("vous jetez") ||
               textLower.Contains("du würfelst") ||
               textLower.Contains("ダイスを振り") || textLower.Contains("を出した");
    }

    private bool IsLocalPlayerRoll(string textLower) {
        return textLower.Contains("you roll") ||
               textLower.Contains("vous obtenez") || textLower.Contains("vous jetez") ||
               textLower.Contains("du würfelst") ||
               textLower.Contains("を出した");
    }

    private void OnChatMessage(IChatMessage message) {
        if (message.Message == null) return;

        string messageText = message.Message.TextValue;
        string textLower = messageText.ToLowerInvariant();
        bool isDiceRollLogKind = false;

        try {
            isDiceRollLogKind = (int)message.LogKind == 73;
        }
        catch { }

        if (isDiceRollLogKind || this.IsFallbackDiceRoll(textLower)) {
            this.HandleDiceRoll(message, messageText, textLower);
            return;
        }

        string senderName = this.nameNormalizer.Normalize(message.Sender?.TextValue ?? string.Empty);
        if (string.IsNullOrEmpty(senderName)) return;

        if (this.RestrictToParticipants && !this.participants.Contains(senderName)) return;

        var channel = message.LogKind.ToGameChatChannel();
        if (channel.HasValue) {
            this.EventFired?.Invoke(new ChatGameEvent {
                Sender = senderName,
                Message = messageText,
                Channel = channel.Value
            });
        }
    }

    private void HandleDiceRoll(IChatMessage message, string messageText, string textLower) {
        this.logger.Debug($"[ChatWatcher] Handling dice roll string: '{messageText}'");

        if (this.diceParser.TryParse(messageText, out int roll, out int maxRoll)) {
            string sender = this.nameNormalizer.Normalize(message.Sender?.TextValue ?? string.Empty);

            if (string.IsNullOrEmpty(sender)) {
                foreach (var payload in message.Message.Payloads) {
                    if (payload is PlayerPayload pp) {
                        sender = this.nameNormalizer.Normalize(pp.PlayerName);
                        break;
                    }
                }
            }

            if (string.IsNullOrEmpty(sender)) {
                foreach (var p in this.participants) {
                    if (!messageText.Contains(p, StringComparison.OrdinalIgnoreCase)) continue;
                    sender = p;
                    break;
                }
            }

            if (string.IsNullOrEmpty(sender) && this.IsLocalPlayerRoll(textLower)) {
                if (this.objectTable.LocalPlayer != null) {
                    sender = this.nameNormalizer.Normalize(this.objectTable.LocalPlayer.Name.TextValue);
                }
            }

            if (!string.IsNullOrEmpty(sender)) {
                this.logger.Info($"[ChatWatcher] Firing DiceRollGameEvent -> Sender: {sender}, Roll: {roll}, MaxRoll: {maxRoll}");
                this.EventFired?.Invoke(new DiceRollGameEvent {
                    Sender = sender,
                    Roll = roll,
                    MaxRoll = maxRoll
                });
            }
            else {
                this.logger.Warning("[ChatWatcher] Failed to resolve a sender for the dice roll.");
            }
        }
        else {
            this.logger.Warning($"[ChatWatcher] Failed to parse dice roll message: '{messageText}'");
        }
    }

    public void Dispose() {
        this.Stop();
    }
}