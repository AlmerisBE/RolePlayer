namespace RolePlayer.API.GameEvents.Services;

using Dalamud.Game;
using Dalamud.Game.Chat;
using Dalamud.Game.Text;
using Dalamud.Game.Text.SeStringHandling.Payloads;
using Dalamud.Plugin.Services;
using RolePlayer.Core.GameEngine.Contracts;
using RolePlayer.Core.GameEngine.Models;
using RolePlayer.Core.Logging.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

public class ChatWatcher : IGameEventWatcher {
    private IChatGui chatGui;
    private IObjectTable objectTable;
    private ILoggerService logger;
    private IClientState clientState;
    private HashSet<string> participants = new(StringComparer.OrdinalIgnoreCase);
    private bool isWatching;

    public event Action<GameEvent>? EventFired;
    public bool RestrictToParticipants { get; set; } = true;

    public ChatWatcher(IChatGui chatGui, IObjectTable objectTable, ILoggerService logger, IClientState clientState) {
        this.chatGui = chatGui;
        this.objectTable = objectTable;
        this.logger = logger;
        this.clientState = clientState;
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

    private bool IsFallbackDiceRoll(string textLower) {
        if (textLower.Contains("random!")) return true;

        return this.clientState.ClientLanguage switch {
            ClientLanguage.English => textLower.Contains("you roll a"),
            ClientLanguage.French => textLower.Contains("lancer d'un dé") || textLower.Contains("vous obtenez") || textLower.Contains("obtient un") || textLower.Contains("vous jetez"),
            ClientLanguage.German => textLower.Contains("du würfelst"),
            ClientLanguage.Japanese => textLower.Contains("ダイスを振り") || textLower.Contains("を出した"),
            _ => false
        };
    }

    private void OnChatMessage(IChatMessage message) {
        if (message.Message == null) return;
        string messageText = message.Message.TextValue;

        bool isDiceRoll = false;
        try {
            isDiceRoll = (int)message.LogKind == 73;
        }
        catch { }

        var textLower = messageText.ToLowerInvariant();

        if (!isDiceRoll && this.IsFallbackDiceRoll(textLower)) {
            isDiceRoll = true;
            this.logger.Debug($"[ChatWatcher] Dice roll detected via text fallback: '{messageText}'");
        }
        else if (isDiceRoll) {
            this.logger.Debug($"[ChatWatcher] Dice roll detected via LogKind 73: '{messageText}'");
        }

        if (isDiceRoll) {
            this.HandleDiceRoll(message, messageText, textLower);
            return;
        }

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

    private void HandleDiceRoll(IChatMessage message, string messageText, string textLower) {
        this.logger.Debug($"[ChatWatcher] Handling dice roll string: '{messageText}'");

        var numbers = Regex.Matches(messageText, @"\d+").Cast<Match>().Select(m => int.Parse(m.Value)).ToList();

        if (numbers.Count >= 2) {
            int roll = 0;
            int outOf = 0;

            var lang = this.clientState.ClientLanguage;

            if (lang == ClientLanguage.English || lang == ClientLanguage.German || textLower.Contains("out of") || textLower.Contains("roll a")) {
                roll = numbers[numbers.Count - 2];
                outOf = numbers[numbers.Count - 1];
            }
            else {
                outOf = numbers[numbers.Count - 2];
                roll = numbers[numbers.Count - 1];
            }

            this.logger.Debug($"[ChatWatcher] Parsed numbers - Roll: {roll}, OutOf: {outOf}");

            string sender = this.CleanPlayerName(message.Sender?.TextValue ?? string.Empty);
            this.logger.Debug($"[ChatWatcher] Initial sender from message.Sender: '{sender}'");

            if (string.IsNullOrEmpty(sender)) {
                foreach (var payload in message.Message.Payloads) {
                    if (payload is PlayerPayload pp) {
                        sender = this.CleanPlayerName(pp.PlayerName);
                        this.logger.Debug($"[ChatWatcher] Resolved sender via PlayerPayload: '{sender}'");
                        break;
                    }
                }
            }

            if (string.IsNullOrEmpty(sender)) {
                foreach (var p in this.participants) {
                    if (messageText.Contains(p, StringComparison.OrdinalIgnoreCase)) {
                        sender = p;
                        this.logger.Debug($"[ChatWatcher] Resolved sender via explicit participant match: '{sender}'");
                        break;
                    }
                }
            }

            if (string.IsNullOrEmpty(sender)) {
                bool isLocalPlayer = lang switch {
                    ClientLanguage.English => textLower.Contains("you roll"),
                    ClientLanguage.French => textLower.Contains("vous obtenez") || textLower.Contains("vous jetez"),
                    ClientLanguage.German => textLower.Contains("du würfelst"),
                    ClientLanguage.Japanese => textLower.Contains("を出した"),
                    _ => false
                };

                if (isLocalPlayer) {
                    var localPlayer = this.objectTable.LocalPlayer;
                    if (localPlayer != null) {
                        sender = this.CleanPlayerName(localPlayer.Name.TextValue);
                        this.logger.Debug($"[ChatWatcher] Resolved sender via local player fallback: '{sender}'");
                    }
                }
            }

            if (!string.IsNullOrEmpty(sender)) {
                this.logger.Info($"[ChatWatcher] Firing DiceRollGameEvent -> Sender: {sender}, Roll: {roll}, OutOf: {outOf}");
                this.EventFired?.Invoke(new DiceRollGameEvent {
                    Sender = sender,
                    Roll = roll,
                    OutOf = outOf
                });
            }
            else {
                this.logger.Warning("[ChatWatcher] Failed to resolve a sender for the dice roll. Event aborted.");
            }
        }
        else {
            this.logger.Warning($"[ChatWatcher] Failed to extract at least 2 numbers from dice roll message: '{messageText}'");
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