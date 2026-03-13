using System.Text.Json.Serialization;

namespace AuraFlow.Core.Models.Api.OpenArt;

public class OpenArtSearchResponse
{
    [JsonPropertyName("items")]
    public IEnumerable<OpenArtSearchResult> Items { get; set; }

    [JsonPropertyName("total")]
    public int Total { get; set; }

    [JsonPropertyName("nextCursor")]
    public string? NextCursor { get; set; }
}
