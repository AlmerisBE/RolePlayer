namespace RolePlayer.API.GameEvents.Contracts;

public interface IDiceRollParser {
    bool TryParse(string messageText, out int roll, out int maxRoll);
}