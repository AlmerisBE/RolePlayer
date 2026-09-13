namespace RolePlayer.Core.Emotes.Contracts;

using System;

public interface IPlayerStateProvider {
    event Action? PlayerStateValid;
    bool IsPlayerValid { get; }
    bool IsEmoteUnlocked(uint emoteId);
    bool IsEmoteActive(uint emoteId);
    uint GetActiveEmoteId();
}