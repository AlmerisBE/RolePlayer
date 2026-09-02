namespace RolePlayer.API.GameData.Providers;

using RolePlayer.UI.EmoteBrowser.Contracts;

public class MockUnlockSourceProvider : IUnlockSourceProvider {
    public string GetUnlockSource(uint emoteId) {
        return "Source inconnue ou non implémentée. (Ex: Mog Station, Quête...)";
    }
}