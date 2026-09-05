namespace RolePlayer.API.FFXIVCollect.Models;

using System.Collections.Generic;
using System.Text.Json.Serialization;

public class FFXIVCollectEmote {
    [JsonPropertyName("id")]
    public uint Id { get; set; }

    [JsonPropertyName("sources")]
    public List<FFXIVCollectSource> Sources { get; set; } = new();
}