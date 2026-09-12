namespace RolePlayer.UI.EmoteBrowser.Services;

using RolePlayer.Core.Emotes.Models;
using RolePlayer.UI.EmoteBrowser.Contracts;

public class EmoteSelectionState : IEmoteSelectionState {
    public EnrichedEmote? SelectedEmote { get; set; }
}