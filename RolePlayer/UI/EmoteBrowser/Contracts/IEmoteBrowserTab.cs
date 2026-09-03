namespace RolePlayer.UI.EmoteBrowser.Contracts;

public interface IEmoteBrowserTab {
    string TabName { get; }
    int SortOrder { get; }
    bool IsSidePanelOpen { get; }
    void Draw();
    void DrawSidePanel();
}