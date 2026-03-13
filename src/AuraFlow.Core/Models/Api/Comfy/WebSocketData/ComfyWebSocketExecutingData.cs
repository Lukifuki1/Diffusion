using System.Text.Json.Serialization;

namespace AuraFlow.Core.Models.Api.Comfy.WebSocketData;

public class ComfyWebSocketExecutingData
{
    [JsonPropertyName("prompt_id")]
    public string? PromptId { get; set; }

    /// <summary>
    /// When this is null it indicates completed
    /// </summary>
    [JsonPropertyName("node")]
    public string? Node { get; set; }
}
