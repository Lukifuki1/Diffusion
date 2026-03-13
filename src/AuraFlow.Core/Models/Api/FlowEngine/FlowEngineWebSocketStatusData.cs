using System.Text.Json.Serialization;

namespace AuraFlow.Core.Models.Api.Comfy.WebSocketData;

public record ComfyWebSocketStatusData
{
    [JsonPropertyName("status")]
    public required ComfyStatus Status { get; set; }
}
