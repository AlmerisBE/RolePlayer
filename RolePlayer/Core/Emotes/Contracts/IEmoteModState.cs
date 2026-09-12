namespace RolePlayer.Core.Emotes.Contracts;

using System;

public interface IEmoteModState {
    event Action? ModStateChanged;
    string GetModNameModifyingEmote(uint emoteId);
}