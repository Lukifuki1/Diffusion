using System.Text.Json.Serialization;

namespace AuraFlow.Core.Models.Api;

public record CivitFileMetadata
{
    [JsonPropertyName("fp")]
    public string? Fp { get; set; }

    [JsonPropertyName("size")]
    public string? Size { get; set; }

    [JsonPropertyName("format")]
    public CivitModelFormat? Format { get; set; }
}
