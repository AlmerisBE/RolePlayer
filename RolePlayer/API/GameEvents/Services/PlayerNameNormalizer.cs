namespace RolePlayer.API.GameEvents.Services;

using RolePlayer.API.GameEvents.Contracts;
using System.Text.RegularExpressions;

public class PlayerNameNormalizer : IPlayerNameNormalizer {
    public string Normalize(string name) {
        if (string.IsNullOrEmpty(name)) return string.Empty;

        // Replace the native cross-world icon with the standard @ separator
        string cleanName = name.Replace('\uE05D', '@');

        // Remove non-letter prefixes (like GM icons or other rogue symbols)
        cleanName = Regex.Replace(cleanName, @"^[^\p{L}]+", "");

        return cleanName.Trim();
    }
}