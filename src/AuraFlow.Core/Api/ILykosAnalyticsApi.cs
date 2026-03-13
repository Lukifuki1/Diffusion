using System.ComponentModel;
using Refit;
using AuraFlow.Core.Models.Api.AuraCloud;
using AuraFlow.Core.Models.Api.AuraCloud.Analytics;

namespace AuraFlow.Core.Api;

[Localizable(false)]
[Headers("User-Agent: AuraFlow")]
public interface IAuraCloudAnalyticsApi
{
    [Post("/api/analytics")]
    Task PostInstallData([Body] AnalyticsRequest data, CancellationToken cancellationToken = default);
}
