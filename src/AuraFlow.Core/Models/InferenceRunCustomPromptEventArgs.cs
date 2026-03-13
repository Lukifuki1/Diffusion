using AuraFlow.Core.Models.Api.Comfy.Nodes;

namespace AuraFlow.Core.Models;

public class InferenceQueueCustomPromptEventArgs : EventArgs
{
    public ComfyNodeBuilder Builder { get; } = new();

    public NodeDictionary Nodes => Builder.Nodes;

    public long? SeedOverride { get; init; }

    public List<(string SourcePath, string DestinationRelativePath)> FilesToTransfer { get; init; } = [];
}
