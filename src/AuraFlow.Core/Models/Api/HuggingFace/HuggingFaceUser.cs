using System.Text.Json.Serialization;

namespace AuraFlow.Core.Models.Api.HuggingFace;

public record HuggingFaceUser
{
    [JsonPropertyName("name")]
    public string? Name { get; init; }
}
