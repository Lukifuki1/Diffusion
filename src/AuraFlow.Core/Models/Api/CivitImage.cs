using System.Text.Json.Serialization;

namespace AuraFlow.Core.Models.Api;

public class CivitImage
{
    [JsonPropertyName("url")]
    public string Url { get; set; }

    [JsonPropertyName("nsfwLevel")]
    public int? NsfwLevel { get; set; }

    [JsonPropertyName("width")]
    public int Width { get; set; }

    [JsonPropertyName("height")]
    public int Height { get; set; }

    [JsonPropertyName("hash")]
    public string Hash { get; set; }

    [JsonPropertyName("type")]
    public string Type { get; set; }

    // Added meta object for additional image metadata
    [JsonPropertyName("meta")]
    public Dictionary<string, object>? Meta { get; set; }
}
