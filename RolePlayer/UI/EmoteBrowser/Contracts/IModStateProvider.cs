namespace RolePlayer.UI.EmoteBrowser.Contracts;

using System;

public interface IModStateProvider {
    event Action? ModStateChanged;
    string GetModNameModifyingEmote(uint emoteId);
}