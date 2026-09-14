namespace RolePlayer.Core.Emotes.Contracts;

using System;

public interface IPlayerUnlockState {
    event Action? PlayerStateValid;
    bool IsPlayerValid { get; }
    bool IsEmoteUnlocked(uint emoteId);
}