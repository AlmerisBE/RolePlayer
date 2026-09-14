namespace RolePlayer.Tests.API.GameEvents.Services;

using RolePlayer.API.GameEvents.Services;
using Xunit;

public class PlayerNameNormalizerTests {
    [Theory]
    [InlineData("Ysaline Sylv'anir", "Ysaline Sylv'anir")]
    [InlineData("Ysaline Sylv'anir@Moogle", "Ysaline Sylv'anir")]
    [InlineData("\uE05DYsaline Sylv'anir", "Ysaline Sylv'anir")] // Symbole inter-serveur natif FFXIV
    [InlineData("\uE05DYsaline Sylv'anir@Cerberus", "Ysaline Sylv'anir")]
    [InlineData("", "")]
    [InlineData(null, "")]
    public void Normalize_CleansServerAndSpecialCharacters(string input, string expected) {
        var normalizer = new PlayerNameNormalizer();

        string result = normalizer.Normalize(input);

        Assert.Equal(expected, result);
    }
}