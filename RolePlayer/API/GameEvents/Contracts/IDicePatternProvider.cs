namespace RolePlayer.API.GameEvents.Contracts;

using System.Collections.Generic;

public interface IDicePatternProvider {
    IReadOnlyList<string> GetLocalizedDiceKeywords();
}