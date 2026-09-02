namespace RolePlayer.UI.EmoteBrowser.Services;

using RolePlayer.UI.EmoteBrowser.Contracts;
using RolePlayer.UI.EmoteBrowser.Models;

public class EmoteSelectionState : IEmoteSelectionState {
    public EmoteDisplayData? SelectedEmote { get; set; }
}