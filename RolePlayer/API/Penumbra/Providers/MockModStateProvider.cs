namespace RolePlayer.API.Penumbra.Providers;

using RolePlayer.UI.EmoteBrowser.Contracts;

public class MockModStateProvider : IModStateProvider {
    public string GetModNameModifyingEmote(uint emoteId) {
        // Simule un mod aléatoire pour certaines emotes (par exemple si l'ID est pair)
        if (emoteId % 2 == 0) {
            return "High Definition Emotes Mod";
        }

        return string.Empty;
    }
}