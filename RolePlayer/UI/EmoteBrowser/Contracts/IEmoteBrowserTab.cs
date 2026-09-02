namespace RolePlayer.UI.EmoteBrowser.Contracts;

public interface IEmoteBrowserTab {
    string TabName { get; }
    void Draw();
}