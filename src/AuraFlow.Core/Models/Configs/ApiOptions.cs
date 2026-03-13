namespace AuraFlow.Core.Models.Configs;

/// <summary>
/// Configuration options for API services
/// </summary>
public record ApiOptions
{
    /// <summary>
    /// Base URL for AuraCloud Authentication API
    /// </summary>
    public Uri AuraCloudAuthApiBaseUrl { get; init; } = new("https://auth.lykos.ai");

    /// <summary>
    /// Base URL for AuraCloud Analytics API
    /// </summary>
    public Uri AuraCloudAnalyticsApiBaseUrl { get; init; } = new("https://analytics.lykos.ai");

    /// <summary>
    /// Base URL for AuraCloud Account API
    /// </summary>
    public Uri AuraCloudAccountApiBaseUrl { get; init; } = new("https://account.lykos.ai/");

    /// <summary>
    /// Base URL for PromptGen API
    /// </summary>
    public Uri AuraCloudPromptGenApiBaseUrl { get; init; } = new("https://promptgen.lykos.ai/api");
}
