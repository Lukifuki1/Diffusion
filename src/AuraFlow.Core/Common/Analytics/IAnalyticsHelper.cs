using AuraFlow.Core.Models.Api.Lykos.Analytics;

namespace AuraFlow.Core.Helper.Analytics;

public interface IAnalyticsHelper
{
    Task TrackPackageInstallAsync(string packageName, string packageVersion, bool isSuccess);

    Task TrackFirstTimeInstallAsync(
        string? selectedPackageName,
        IEnumerable<string>? selectedRecommendedModels,
        bool firstTimeSetupSkipped
    );

    Task TrackAsync(AnalyticsRequest data);
}
