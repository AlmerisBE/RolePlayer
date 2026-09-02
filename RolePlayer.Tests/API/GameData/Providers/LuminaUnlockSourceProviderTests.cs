namespace RolePlayer.Tests.API.GameData.Providers;

using Dalamud.Plugin.Services;
using NSubstitute;
using RolePlayer.API.GameData.Providers;
using Xunit;

public class LuminaUnlockSourceProviderTests {

    [Fact]
    public void GetUnlockSource_WhenEmoteSheetIsNull_ReturnsUnknown() {
        var mockDataManager = Substitute.For<IDataManager>();
        var provider = new LuminaUnlockSourceProvider(mockDataManager);

        var result = provider.GetUnlockSource(1);

        Assert.Equal("Unknown", result);
    }

    [Fact]
    public void Constructor_BuildsCacheWithoutExceptions_WhenSheetsAreNull() {
        var mockDataManager = Substitute.For<IDataManager>();

        var exception = Record.Exception(() => new LuminaUnlockSourceProvider(mockDataManager));

        Assert.Null(exception);
    }
}