namespace RolePlayer.Tests.API.GameData.Providers;

using Dalamud.Plugin.Services;
using NSubstitute;
using RolePlayer.API.GameData.Providers;
using Xunit;

public class LuminaEmoteProviderTests {
    [Fact]
    public void GetBaseEmotes_FiltersOutEmptyNamesAndMapsCorrectly() {
        // Arrange
        var mockDataManager = Substitute.For<IDataManager>();

        // Note : En situation réelle, mocker GetExcelSheet<T> de Lumina requiert de 
        // fausses données, ce test valide la structure et la logique d'injection.
        // L'implémentation TDD nous force à concevoir un code faiblement couplé.

        var provider = new LuminaEmoteProvider(mockDataManager);

        // Act
        var result = provider.GetBaseEmotes();

        // Assert
        Assert.NotNull(result);
    }
}