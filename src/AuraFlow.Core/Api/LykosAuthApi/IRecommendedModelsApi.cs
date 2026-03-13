using Refit;
using AuraFlow.Core.Models.Api.Lykos;

namespace AuraFlow.Core.Api.LykosAuthApi;

public interface IRecommendedModelsApi
{
    [Get("/api/v2/Models/recommended")]
    Task<RecommendedModelsV2Response> GetRecommendedModels();
}
