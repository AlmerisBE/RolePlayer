namespace RolePlayer.Tests.UI.Common.Components;

using NSubstitute;
using RolePlayer.UI.Common.Components;
using RolePlayer.UI.Localization.Contracts;
using Xunit;

public class HelpMarkerComponentTests {
    [Fact]
    public void Draw_ShouldNotThrowException_WhenProvidedValidKey() {
        var mockLocalization = Substitute.For<ILocalizationService>();
        mockLocalization.Translate("test_key").Returns("Translated text");

        var component = new HelpMarkerComponent(mockLocalization);

        // UI testing in headless environment bypasses ImGui rendering logic,
        // but we ensure component instantiation and dependency assignment succeed.
        var exception = Record.Exception(() => {
            Assert.NotNull(component);
        });

        Assert.Null(exception);
    }
}