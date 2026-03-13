using System.Text.Json.Serialization;

namespace AuraFlow.Core.Models.Api;

public class ProgressRequest
{
    [JsonPropertyName("skip_current_image")]
    public bool? SkipCurrentImage { get; set; }
}
