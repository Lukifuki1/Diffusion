using System.Text.Json.Serialization;
using Semver;
using AuraFlow.Core.Converters.Json;

namespace AuraFlow.Core.Models.Settings;

public class AnalyticsSettings
{
    [JsonIgnore]
    public static TimeSpan DefaultLaunchDataSendInterval { get; set; } = TimeSpan.FromDays(1);

    [JsonConverter(typeof(SemVersionJsonConverter))]
    public SemVersion? LastSeenConsentVersion { get; set; }

    public bool? LastSeenConsentAccepted { get; set; }

    public bool IsUsageDataEnabled { get; set; }

    public DateTimeOffset? LaunchDataLastSentAt { get; set; }
}
