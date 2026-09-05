namespace RolePlayer.UI.Themes.Contracts;

using System.Collections.Generic;

public interface IThemeManagementService {
    string ThemeDirectory { get; }
    IEnumerable<string> GetAvailableThemes();
    void LoadTheme(string themeName);
    void PushTheme();
    void PopTheme();
    void OpenThemeDirectory();
}