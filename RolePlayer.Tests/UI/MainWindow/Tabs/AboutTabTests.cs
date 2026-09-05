namespace RolePlayer.Tests.UI.MainWindow.Tabs;

using Dalamud.Plugin;
using NSubstitute;
using RolePlayer.UI.Localization.Contracts;
using RolePlayer.UI.MainWindow.Tabs;
using Xunit;

public class AboutTabTests {
    [Fact]
    public void AboutTab_Initialization_SetsPropertiesCorrectly() {
        var mockLocalization = Substitute.For<ILocalizationService>();
        var mockPluginInterface = Substitute.For<IDalamudPluginInterface>();

        mockLocalization.Translate("about_tab_name").Returns("About");

        var tab = new AboutTab(mockLocalization, mockPluginInterface);

        Assert.Equal("About", tab.TabName);
        Assert.Equal(999, tab.SortOrder);
        Assert.False(tab.IsSidePanelOpen);
    }
}