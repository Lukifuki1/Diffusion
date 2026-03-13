using System.Text.Json.Serialization;

namespace AuraFlow.Core.Models.Api.AuraCloud.Analytics;

public class AnalyticsRequest
{
    [JsonPropertyName("type")]
    public virtual string Type { get; set; } = "unknown";

    public DateTimeOffset Timestamp { get; set; } = DateTimeOffset.UtcNow;
}
