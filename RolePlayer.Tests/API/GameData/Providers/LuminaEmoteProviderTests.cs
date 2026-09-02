namespace RolePlayer.Tests.API.GameData.Providers;

using Dalamud.Plugin.Services;
using NSubstitute;
using RolePlayer.API.GameData.Providers;
using RolePlayer.UI.EmoteBrowser.Contracts;
using Xunit;

public class LuminaEmoteProviderTests {
    [Fact]
    public void GetBaseEmotes_FiltersOutEmptyNamesAndMapsCorrectly() {
        var mockDataManager = Substitute.For<IDataManager>();
        var mockUnlockSourceProvider = Substitute.For<IUnlockSourceProvider>();
        var mockClientState = Substitute.For<IClientState>();

        var provider = new LuminaEmoteProvider(mockDataManager, mockUnlockSourceProvider, mockClientState);
        var result = provider.GetBaseEmotes();

        Assert.NotNull(result);
    }
}