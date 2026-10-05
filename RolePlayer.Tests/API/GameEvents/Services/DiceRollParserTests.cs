namespace RolePlayer.Tests.API.GameEvents.Services;

using RolePlayer.API.GameEvents.Services;
using Xunit;

public class DiceRollParserTests {
    [Theory]
    [InlineData("You roll a 42.", 42, 999)]
    [InlineData("You roll a 42 (out of 100).", 42, 100)]
    [InlineData("Du würfelst eine 42 (von 100).", 42, 100)]
    [InlineData("Vous jetez les dés (100) et obtenez 42 !", 42, 100)]
    [InlineData("Bob Dinkleberryは100面ダイスで42を出した。", 42, 100)]
    [InlineData("Bob Dinkleberryはダイスで42を出した。", 42, 999)]
    public void TryParse_WithVariousLanguages_ExtractsCorrectValues(string message, int expectedRoll, int expectedMax) {
        var parser = new DiceRollParser();

        bool result = parser.TryParse(message, out int roll, out int maxRoll);

        Assert.True(result);
        Assert.Equal(expectedRoll, roll);
        Assert.Equal(expectedMax, maxRoll);
    }
}