using AuraFlow.Core.Models.Api.Comfy.Nodes;
using AuraFlow.Core.Models.Packages.Extensions;

namespace AuraFlow.Core.Attributes;

/// <summary>
/// Options for <see cref="ComfyTypedNodeBase{TOutput}"/>
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public class TypedNodeOptionsAttribute : Attribute
{
    public string? Name { get; init; }

    public string[]? RequiredExtensions { get; init; }

    public IEnumerable<ExtensionSpecifier> GetRequiredExtensionSpecifiers()
    {
        return RequiredExtensions?.Select(ExtensionSpecifier.Parse) ?? Enumerable.Empty<ExtensionSpecifier>();
    }
}
