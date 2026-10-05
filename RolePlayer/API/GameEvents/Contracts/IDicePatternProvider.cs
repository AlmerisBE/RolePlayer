namespace RolePlayer.API.GameEvents.Contracts;

using System.Collections.Generic;
using System.Text.RegularExpressions;

public interface IDicePatternProvider {
    IReadOnlyList<Regex> GetLocalizedDicePatterns();
}