using AuraFlow.Core.Models.Api.Comfy.WebSocketData;

namespace AuraFlow.Core.Exceptions;

public class ComfyNodeException : Exception
{
    public required ComfyWebSocketExecutionErrorData ErrorData { get; init; }
    public required string JsonData { get; init; }
}
