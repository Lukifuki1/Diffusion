using System.ComponentModel;
using Refit;
using AuraFlow.Core.Models.Api.Lykos;
using AuraFlow.Core.Models.Api.Lykos.Analytics;

namespace AuraFlow.Core.Api;

[Localizable(false)]
[Headers("User-Agent: StabilityMatrix")]
public interface ILykosAnalyticsApi
{
    [Post("/api/analytics")]
    Task PostInstallData([Body] AnalyticsRequest data, CancellationToken cancellationToken = default);
}
