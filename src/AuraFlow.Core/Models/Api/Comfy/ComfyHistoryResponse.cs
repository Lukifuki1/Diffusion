using System.Text.Json.Serialization;

namespace AuraFlow.Core.Models.Api.Comfy;

public class ComfyHistoryResponse
{
    [JsonPropertyName("outputs")]
    public required Dictionary<string, ComfyHistoryOutput> Outputs { get; set; }
}
