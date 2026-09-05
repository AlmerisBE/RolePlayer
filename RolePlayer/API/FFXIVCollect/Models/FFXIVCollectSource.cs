namespace RolePlayer.API.FFXIVCollect.Models;

using System.Text.Json.Serialization;

public class FFXIVCollectSource {
    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty;

    [JsonPropertyName("text")]
    public string Text { get; set; } = string.Empty;
}