namespace RolePlayer.Tests.API.GameData.Providers;

using Dalamud.Plugin.Services;
using NSubstitute;
using RolePlayer.API.GameData.Providers;
using RolePlayer.UI.Localization.Contracts;
using Xunit;

public class LuminaUnlockSourceProviderTests {

    [Fact]
    public void GetUnlockSource_WhenEmoteSheetIsNull_ReturnsUnknown() {
        var mockDataManager = Substitute.For<IDataManager>();
        var mockLocalization = Substitute.For<ILocalizationService>();
        mockLocalization.Translate(Arg.Any<string>()).Returns(callInfo => callInfo.Arg<string>());

        var provider = new LuminaUnlockSourceProvider(mockDataManager, mockLocalization);

        var result = provider.GetUnlockSource(1);

        Assert.Equal("src_unknown", result);
    }

    [Fact]
    public void Constructor_BuildsCacheWithoutExceptions_WhenSheetsAreNull() {
        var mockDataManager = Substitute.For<IDataManager>();
        var mockLocalization = Substitute.For<ILocalizationService>();

        var exception = Record.Exception(() => new LuminaUnlockSourceProvider(mockDataManager, mockLocalization));

        Assert.Null(exception);
    }
}