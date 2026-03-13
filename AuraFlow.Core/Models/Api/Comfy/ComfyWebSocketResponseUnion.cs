using System.Net.WebSockets;

namespace AuraFlow.Core.Models.Api.Comfy;

public record ComfyWebSocketResponseUnion
{
    public WebSocketMessageType MessageType { get; set; }
    public ComfyWebSocketResponse? Json { get; set; }
    public byte[]? Bytes { get; set; }
};


