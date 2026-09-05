namespace RolePlayer.Tests.API.FFXIVCollect.Providers;

using Dalamud.Game;
using Dalamud.Plugin.Services;
using NSubstitute;
using RolePlayer.API.FFXIVCollect.Providers;
using RolePlayer.API.GameData.Providers;
using RolePlayer.Core.Logging.Contracts;
using RolePlayer.UI.Localization.Contracts;
using Xunit;

public class FFXIVCollectUnlockSourceProviderTests {

    [Fact]
    public void GetUnlockSource_WhenExternalCacheIsEmpty_FallsBackToLuminaProvider() {
        var mockDataManager = Substitute.For<IDataManager>();
        var mockLocalization = Substitute.For<ILocalizationService>();
        var mockClientState = Substitute.For<IClientState>();
        var mockLogger = Substitute.For<ILoggerService>();

        mockLocalization.Translate(Arg.Any<string>()).Returns("Fallback Value");
        mockClientState.ClientLanguage.Returns(ClientLanguage.French);

        var fallbackProvider = new LuminaUnlockSourceProvider(mockDataManager, mockLocalization);
        using var provider = new FFXIVCollectUnlockSourceProvider(fallbackProvider, mockClientState, mockLogger);

        var result = provider.GetUnlockSource(9999);

        Assert.Equal("Fallback Value", result);
    }
}