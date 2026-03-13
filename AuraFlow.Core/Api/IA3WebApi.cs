using Refit;
using AuraFlow.Core.Models.Api;

namespace AuraFlow.Core.Api;

[Headers("User-Agent: AuraFlow")]
public interface IA3WebApi
{
    [Get("/internal/ping")]
    Task<string> GetPing(CancellationToken cancellationToken = default);
    
    [Post("/sdapi/v1/txt2img")]
    Task<ImageResponse> TextToImage([Body] TextToImageRequest request, CancellationToken cancellationToken = default);
    
    [Get("/sdapi/v1/progress")]
    Task<ProgressResponse> GetProgress([Body] ProgressRequest request, CancellationToken cancellationToken = default);
    
    [Get("/sdapi/v1/options")]
    Task<A3Options> GetOptions(CancellationToken cancellationToken = default);
    
    [Post("/sdapi/v1/options")]
    Task SetOptions([Body] A3Options request, CancellationToken cancellationToken = default);
}
