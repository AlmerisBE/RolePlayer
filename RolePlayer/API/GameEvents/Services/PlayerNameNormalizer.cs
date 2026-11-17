namespace RolePlayer.API.GameEvents.Services;

using RolePlayer.API.GameEvents.Contracts;
using System.Text.RegularExpressions;

public class PlayerNameNormalizer : IPlayerNameNormalizer {
    public string Normalize(string name) {
        if (string.IsNullOrEmpty(name)) return string.Empty;

        // FFXIV uses \uE05D for the cross-world icon, and @ is used as a fallback/manual separator
        var parts = name.Split(new[] { '\uE05D', '@' }, 2);
        var cleanName = parts[0];

        // Remove non-letter prefixes (like GM icons or other rogue symbols)
        cleanName = Regex.Replace(cleanName, @"^[^\p{L}]+", "");

        return cleanName.Trim();
    }
}