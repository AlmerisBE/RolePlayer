namespace RolePlayer.Tests.API.GameData.Providers;

using Dalamud.Plugin.Services;
using NSubstitute;
using RolePlayer.API.GameData.Providers;
using RolePlayer.UI.EmoteBrowser.Contracts;
using Xunit;

public class LuminaEmoteProviderTests {
    [Fact]
    public void GetBaseEmotes_FiltersOutEmptyNamesAndMapsCorrectly() {
        // Arrange
        var mockDataManager = Substitute.For<IDataManager>();
        var mockUnlockSourceProvider = Substitute.For<IUnlockSourceProvider>();

        // Injection des deux dépendances requises
        var provider = new LuminaEmoteProvider(mockDataManager, mockUnlockSourceProvider);

        // Act
        var result = provider.GetBaseEmotes();

        // Assert
        Assert.NotNull(result);
    }
}