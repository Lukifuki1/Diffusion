using Refit;
using AuraFlow.Core.Models.Api;

namespace AuraFlow.Core.Api;

public interface ILykosModelDiscoveryApi
{
    [Get("/civit/models")]
    Task<CivitModelsResponse> GetModels(
        [Query] CivitModelsRequest request,
        [Header("X-Transcode-Video-To-Image")] bool? transcodeVideoToImage = null,
        [Header("X-Transcode-Anim-To-Image")] bool? transcodeAnimToImage = null
    );

    [Get("/civit/models/{id}")]
    Task<CivitModel> GetModelById([AliasAs("id")] int id);
}
