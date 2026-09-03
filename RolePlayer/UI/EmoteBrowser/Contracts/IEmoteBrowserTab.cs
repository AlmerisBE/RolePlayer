namespace RolePlayer.UI.EmoteBrowser.Contracts;

public interface IEmoteBrowserTab {
    string TabName { get; }
    int SortOrder { get; }
    void Draw();
}