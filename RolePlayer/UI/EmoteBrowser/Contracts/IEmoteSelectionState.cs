namespace RolePlayer.UI.EmoteBrowser.Contracts;

using RolePlayer.UI.EmoteBrowser.Models;

public interface IEmoteSelectionState {
    EmoteDisplayData? SelectedEmote { get; set; }
}