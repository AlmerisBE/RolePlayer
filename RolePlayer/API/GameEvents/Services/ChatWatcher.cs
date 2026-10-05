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
using System.Text.RegularExpressions;

public class ChatWatcher : IGameEventWatcher {
    private IChatGui chatGui;
    private IObjectTable objectTable;
    private ILoggerService logger;
    private IDiceRollParser diceParser;
    private IPlayerNameNormalizer nameNormalizer;
    private IDicePatternProvider dicePatternProvider;
    private HashSet<string> participants = new(StringComparer.OrdinalIgnoreCase);
    private IReadOnlyList<Regex> dynamicDicePatterns;
    private bool isWatching;

    public event Action<GameEvent>? EventFired;
    public bool RestrictToParticipants { get; set; } = true;

    public ChatWatcher(
        IChatGui chatGui,
        IObjectTable objectTable,
        ILoggerService logger,
        IDiceRollParser diceParser,
        IPlayerNameNormalizer nameNormalizer,
        IDicePatternProvider dicePatternProvider) {

        this.chatGui = chatGui;
        this.objectTable = objectTable;
        this.logger = logger;
        this.diceParser = diceParser;
        this.nameNormalizer = nameNormalizer;
        this.dicePatternProvider = dicePatternProvider;

        this.dynamicDicePatterns = this.dicePatternProvider.GetLocalizedDicePatterns();
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

    private bool IsFallbackDiceRoll(string messageText) {
        if (this.dynamicDicePatterns == null || this.dynamicDicePatterns.Count == 0) {
            string lowerText = messageText.ToLowerInvariant();
            return lowerText.Contains("you roll") || lowerText.Contains("obtenez") || lowerText.Contains("würfelst");
        }

        foreach (var regex in this.dynamicDicePatterns) {
            if (regex.IsMatch(messageText)) return true;
        }

        return false;
    }

    private bool IsLocalPlayerRoll(string textLower) {
        return textLower.Contains("you roll") ||
               textLower.Contains("vous obtenez") ||
               textLower.Contains("vous jetez") ||
               textLower.Contains("du würfelst") ||
               textLower.Contains("を出した");
    }

    private void OnChatMessage(IChatMessage message) {
        if (message.Message == null) return;

        string messageText = message.Message.TextValue;
        string textLower = messageText.ToLowerInvariant();

        if (messageText.TrimStart().StartsWith("[")) return;

        var channel = message.LogKind.ToGameChatChannel();

        // If the message is from a standard player channel (Say, Party, etc.), it cannot be a system dice roll.
        // This completely prevents spoofing where a player types the exact system string in chat.
        if (channel.HasValue) {
            string senderName = this.nameNormalizer.Normalize(message.Sender?.TextValue ?? string.Empty);
            if (string.IsNullOrEmpty(senderName)) return;

            if (this.RestrictToParticipants && !this.participants.Contains(senderName)) return;

            this.EventFired?.Invoke(new ChatGameEvent {
                Sender = senderName,
                Message = messageText,
                Channel = channel.Value
            });

            return;
        }

        // If we reach this point, it is a system message (channel == null).
        // It is now safe to evaluate it against dice roll patterns.
        bool isDiceRollLogKind = false;

        try {
            int logKind = (int)message.LogKind;
            isDiceRollLogKind = logKind == 73 || logKind == 74 || logKind == 2122 || logKind == 2123;
        }
        catch { }

        if (isDiceRollLogKind || this.IsFallbackDiceRoll(messageText)) {
            this.HandleDiceRoll(message, messageText, textLower);
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