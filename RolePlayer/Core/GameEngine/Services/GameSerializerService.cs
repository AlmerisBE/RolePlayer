namespace RolePlayer.Core.GameEngine.Services;

using RolePlayer.Core.GameEngine.Contracts;
using RolePlayer.Core.GameEngine.Models;
using System;
using System.Text;
using System.Text.Json;

public class GameSerializerService : IGameSerializerService {
    private JsonSerializerOptions jsonOptions;

    public GameSerializerService() {
        this.jsonOptions = new JsonSerializerOptions {
            WriteIndented = true,
            PropertyNameCaseInsensitive = true
        };
    }

    public string Serialize(GameDefinition game) {
        if (game == null) return string.Empty;
        return JsonSerializer.Serialize(game, this.jsonOptions);
    }

    public GameDefinition? Deserialize(string json) {
        if (string.IsNullOrWhiteSpace(json)) return null;

        try {
            return JsonSerializer.Deserialize<GameDefinition>(json, this.jsonOptions);
        }
        catch (JsonException) {
            return null;
        }
    }

    public string ToBase64Export(GameDefinition game) {
        if (game == null) return string.Empty;

        var json = JsonSerializer.Serialize(game);
        var bytes = Encoding.UTF8.GetBytes(json);
        return Convert.ToBase64String(bytes);
    }

    public GameDefinition? FromBase64Import(string base64Data) {
        if (string.IsNullOrWhiteSpace(base64Data)) return null;

        try {
            var bytes = Convert.FromBase64String(base64Data);
            var json = Encoding.UTF8.GetString(bytes);
            return this.Deserialize(json);
        }
        catch (FormatException) {
            return null;
        }
    }
}