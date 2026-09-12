namespace RolePlayer.Core.Emotes.Services;

using RolePlayer.Core.Emotes.Contracts;
using RolePlayer.Core.Emotes.Models;
using RolePlayer.Core.Logging.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public class EmoteCacheService : IEmoteCache, IDisposable {
    private IRawEmoteRepository emoteRepository;
    private IPlayerUnlockState playerState;
    private IEmoteModState modState;
    private ILoggerService logger;

    private List<EnrichedEmote> cache = new();
    public bool IsReady { get; private set; } = false;

    public event Action? CacheUpdated;

    public EmoteCacheService(
        IRawEmoteRepository emoteRepository,
        IPlayerUnlockState playerState,
        IEmoteModState modState,
        ILoggerService logger) {

        this.emoteRepository = emoteRepository;
        this.playerState = playerState;
        this.modState = modState;
        this.logger = logger;

        this.modState.ModStateChanged += this.ForceRefresh;
        this.playerState.PlayerStateValid += this.ForceRefresh;

        this.ForceRefresh();
    }

    public IReadOnlyList<EnrichedEmote> GetCachedEmotes() => this.cache;

    public void ForceRefresh() {
        if (!this.playerState.IsPlayerValid) return;

        Task.Run(() => {
            try {
                var baseEmotes = this.emoteRepository.GetBaseEmotes().ToList();
                var newCache = new List<EnrichedEmote>();

                foreach (var emote in baseEmotes) {
                    emote.IsUnlocked = !emote.IsUnlockable || this.playerState.IsEmoteUnlocked(emote.Id);
                    var modName = this.modState.GetModNameModifyingEmote(emote.Id);
                    emote.IsModded = !string.IsNullOrEmpty(modName);
                    emote.ModName = modName;

                    newCache.Add(emote);
                }

                this.cache = newCache;
                this.IsReady = true;
                this.CacheUpdated?.Invoke();
            }
            catch (Exception ex) {
                this.logger.Error(ex, "[EmoteCacheService] Background emote resolution failed.");
            }
        });
    }

    public void Dispose() {
        this.modState.ModStateChanged -= this.ForceRefresh;
        this.playerState.PlayerStateValid -= this.ForceRefresh;
    }
}