namespace RolePlayer.Tests.API.GameEvents.Services;

using RolePlayer.API.GameEvents.Services;
using Xunit;

public class DiceRollParserTests {
    [Theory]
    [InlineData("Random! You roll a 42 (out of 100).", 42, 100)] // Format Anglais
    [InlineData("Vous jetez un dé 100 ! Vous obtenez un 42 !", 42, 100)] // Format Français
    [InlineData("Vous jetez les dés et obtenez 569 !", 569, 999)] // Lancer français sans max spécifié (BUG FIX 999)
    [InlineData("Vous jetez les dés et obtenez 0 !", 0, 999)] // Test explicite pour le 0
    [InlineData("Du würfelst eine 42 (W100).", 42, 100)] // Format Allemand
    [InlineData("Almerisはダイスを振った！ 42を出した！", 42, 999)] // Exemple basique JP
    [InlineData("Un message sans nombre", -1, -1)] // Invalide
    public void TryParse_EvaluatesLanguagePatternsCorrectly(string message, int expectedRoll, int expectedMax) {
        var parser = new DiceRollParser();

        bool success = parser.TryParse(message, out int roll, out int maxRoll);

        if (expectedRoll >= 0 && expectedMax >= 0) { // On autorise >= 0 car un score de 0 est possible
            Assert.True(success);
            Assert.Equal(expectedRoll, roll);
            Assert.Equal(expectedMax, maxRoll);
        }
        else {
            Assert.False(success);
        }
    }
}