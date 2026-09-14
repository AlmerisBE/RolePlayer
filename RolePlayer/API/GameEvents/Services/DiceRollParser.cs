namespace RolePlayer.API.GameEvents.Services;

using RolePlayer.API.GameEvents.Contracts;
using System;
using System.Text.RegularExpressions;

public class DiceRollParser : IDiceRollParser {
    public bool TryParse(string messageText, out int roll, out int maxRoll) {
        roll = 0;
        maxRoll = 0;

        if (string.IsNullOrWhiteSpace(messageText)) return false;

        var matches = Regex.Matches(messageText, @"\d+");
        if (matches.Count < 2) return false;

        int firstNum = int.Parse(matches[matches.Count - 2].Value);
        int secondNum = int.Parse(matches[matches.Count - 1].Value);

        bool isFrenchFormat = messageText.Contains("dé ", StringComparison.OrdinalIgnoreCase) ||
                              messageText.Contains("obtenez", StringComparison.OrdinalIgnoreCase) ||
                              messageText.Contains("obtient", StringComparison.OrdinalIgnoreCase) ||
                              messageText.Contains("jetez", StringComparison.OrdinalIgnoreCase);

        if (isFrenchFormat) {
            maxRoll = firstNum;
            roll = secondNum;
        }
        else {
            roll = firstNum;
            maxRoll = secondNum;
        }

        return true;
    }
}