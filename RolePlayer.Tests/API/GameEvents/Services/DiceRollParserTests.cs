namespace RolePlayer.Tests.API.GameEvents.Services;

using RolePlayer.API.GameEvents.Services;
using Xunit;

public class DiceRollParserTests {
    [Theory]
    [InlineData("Random! You roll a 42 (out of 100).", 42, 100)] // Format Anglais
    [InlineData("Vous jetez un dé 100 ! Vous obtenez un 42 !", 42, 100)] // Format Français
    [InlineData("Du würfelst eine 42 (W100).", 42, 100)] // Format Allemand
    [InlineData("Almerisはダイスを振った！ 42を出した！", 42, 0)] // Exemple basique JP (Fallback si les nombres manquent)
    [InlineData("Un message sans nombre", 0, 0)] // Invalide
    [InlineData("Juste un 42", 0, 0)] // Invalide (1 seul nombre)
    public void TryParse_EvaluatesLanguagePatternsCorrectly(string message, int expectedRoll, int expectedMax) {
        var parser = new DiceRollParser();

        bool success = parser.TryParse(message, out int roll, out int maxRoll);

        if (expectedRoll > 0 && expectedMax > 0) {
            Assert.True(success);
            Assert.Equal(expectedRoll, roll);
            Assert.Equal(expectedMax, maxRoll);
        }
        else {
            Assert.False(success);
        }
    }
}