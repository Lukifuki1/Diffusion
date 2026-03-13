using System.Text.Json.Serialization;

namespace AuraFlow.Core.Models.Api.AuraCloud.Analytics;

public class PackageInstallAnalyticsRequest : AnalyticsRequest
{
    public required string PackageName { get; set; }

    public required string PackageVersion { get; set; }

    public bool IsSuccess { get; set; }

    [JsonPropertyName("type")]
    public override string Type { get; set; } = "package-install";
}
