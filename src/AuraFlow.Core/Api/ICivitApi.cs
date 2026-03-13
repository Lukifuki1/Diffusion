using System.Text.Json.Nodes;
using Refit;
using AuraFlow.Core.Models.Api;

namespace AuraFlow.Core.Api;

[Headers("User-Agent: AuraFlow/1.0")]
public interface ICivitApi
{
    [Get("/api/v1/models")]
    Task<CivitModelsResponse> GetModels(CivitModelsRequest request);

    [Get("/api/v1/models/{id}")]
    Task<CivitModel> GetModelById([AliasAs("id")] int id);

    [Get("/api/v1/model-versions/by-hash/{hash}")]
    Task<CivitModelVersionResponse> GetModelVersionByHash([Query] string hash);

    [Get("/api/v1/model-versions/{id}")]
    Task<CivitModelVersion> GetModelVersionById(int id);

    [Get("/api/v1/models?baseModels=gimmethelist")]
    Task<HttpResponseMessage> GetBaseModelList();
}
