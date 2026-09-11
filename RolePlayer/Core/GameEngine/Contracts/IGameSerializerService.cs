namespace RolePlayer.Core.GameEngine.Contracts;

using RolePlayer.Core.GameEngine.Models;

public interface IGameSerializerService {
    string Serialize(GameDefinition game);
    GameDefinition? Deserialize(string json);
    string ToBase64Export(GameDefinition game);
    GameDefinition? FromBase64Import(string base64Data);
}