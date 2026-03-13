using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;
using AuraFlow.Core.Models.Api.Comfy.NodeTypes;

namespace AuraFlow.Core.Models.Api.Comfy.Nodes;

[JsonSerializable(typeof(ComfyNode))]
[SuppressMessage("ReSharper", "CollectionNeverQueried.Global")]
[SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
public record ComfyNode
{
    [JsonPropertyName("class_type")]
    public required string ClassType { get; init; }
    
    [JsonPropertyName("inputs")]
    public required Dictionary<string, object?> Inputs { get; init; }
    
    public NamedComfyNode ToNamedNode(string name)
    {
        return new NamedComfyNode(name)
        {
            ClassType = ClassType,
            Inputs = Inputs
        };
    } 
}
