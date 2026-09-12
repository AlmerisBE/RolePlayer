namespace RolePlayer.UI.EmoteBrowser.Contracts;

using RolePlayer.Core.Emotes.Models;

public interface IEmoteSelectionState {
    EnrichedEmote? SelectedEmote { get; set; }
}