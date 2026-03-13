using System.Text.Json.Serialization;

namespace AuraFlow.Core.Models.Api.Comfy;

public record ComfyUploadImageResponse
{
    [JsonPropertyName("name")]
    public required string Name { get; set; }

    [JsonPropertyName("type")]
    public required string Type { get; set; }

    [JsonPropertyName("subfolder")]
    public required string SubFolder { get; set; }
}
