using Refit;
using AuraFlow.Core.Models.Api.AuraCloud;

namespace AuraFlow.Core.Api.AuraCloudAuthApi;

public interface IRecommendedModelsApi
{
    [Get("/api/v2/Models/recommended")]
    Task<RecommendedModelsV2Response> GetRecommendedModels();
}
