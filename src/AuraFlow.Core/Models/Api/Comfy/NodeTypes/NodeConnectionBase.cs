using System.Text.Json.Serialization;
using AuraFlow.Core.Converters.Json;

namespace AuraFlow.Core.Models.Api.Comfy.NodeTypes;

[JsonConverter(typeof(NodeConnectionBaseJsonConverter))]
public abstract class NodeConnectionBase
{
    /// <summary>
    /// Array data for the connection.
    /// [(string) Node Name, (int) Connection Index]
    /// </summary>
    public object[]? Data { get; init; }
}
