using System.Text.Json.Serialization;

namespace AuraFlow.Core.Models.Update;

[JsonSerializable(typeof(UpdateManifest))]
public record UpdateManifest
{
    public required Dictionary<UpdateChannel, UpdatePlatforms> Updates { get; init; }
}


// Fixed for .NET 8: Added JsonSourceGenerationOptions with CamelCase naming
[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase)]
[JsonSerializable(typeof(UpdateManifest))]
public partial class UpdateManifestContext : JsonSerializerContext
{
}
