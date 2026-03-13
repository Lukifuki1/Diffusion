namespace AuraFlow.Core.Models.Api.AuraCloud.Analytics;

public class FirstTimeInstallAnalytics : AnalyticsRequest
{
    public string? SelectedPackageName { get; set; }

    public IEnumerable<string>? SelectedRecommendedModels { get; set; }

    public bool FirstTimeSetupSkipped { get; set; }

    public override string Type { get; set; } = "first-time-install";
}
