using Refit;
using AuraFlow.Core.Models.Api.OpenArt;

namespace AuraFlow.Core.Api;

[Headers("User-Agent: StabilityMatrix")]
public interface IOpenArtApi
{
    [Get("/feed")]
    Task<OpenArtSearchResponse> GetFeedAsync([Query] OpenArtFeedRequest request);

    [Get("/list")]
    Task<OpenArtSearchResponse> SearchAsync([Query] OpenArtSearchRequest request);

    [Post("/download")]
    Task<OpenArtDownloadResponse> DownloadWorkflowAsync([Body] OpenArtDownloadRequest request);
}
