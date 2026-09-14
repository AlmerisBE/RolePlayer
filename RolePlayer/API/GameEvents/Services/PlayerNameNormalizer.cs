namespace RolePlayer.API.GameEvents.Services;

using RolePlayer.API.GameEvents.Contracts;
using System.Text.RegularExpressions;

public class PlayerNameNormalizer : IPlayerNameNormalizer {
    public string Normalize(string name) {
        if (string.IsNullOrEmpty(name)) return string.Empty;

        var parts = name.Split('@', 2);
        var cleanName = Regex.Replace(parts[0], @"^[^\p{L}]+", "");

        return cleanName.Trim();
    }
}