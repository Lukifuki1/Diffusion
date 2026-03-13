using System.Text.Json.Serialization;

namespace AuraFlow.Core.Models.Api.Invoke;

public class ScanFolderResult
{
    public string Path { get; set; }

    [JsonPropertyName("is_installed")]
    public bool IsInstalled { get; set; }
}
