using Refit;
using AuraFlow.ChatInterface.Models;

namespace AuraFlow.ChatInterface.Interfaces;

public interface IChatInterfaceApi
{
    [Post("/api/v1/generate")]
    Task<GenerationResponse> PostGenerate([Body] GenerationRequest request);

    [Get("/api/v1/progress/{taskId}")]
    Task<GenerationProgress> GetProgress(string taskId);

    [Post("/api/v1/cancel/{taskId}")]
    Task CancelGeneration(string taskId);
}
