namespace RolePlayer.API.FFXIVCollect.Models;

using System.Collections.Generic;
using System.Text.Json.Serialization;

public class FFXIVCollectResponse {
    [JsonPropertyName("results")]
    public List<FFXIVCollectEmote> Results { get; set; } = new();
}