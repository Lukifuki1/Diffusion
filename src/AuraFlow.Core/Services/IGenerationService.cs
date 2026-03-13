namespace AuraFlow.Core.Services;

/// <summary>
/// Interface for the generation service that handles image and video generation requests
/// </summary>
public interface IGenerationService
{
    /// <summary>
    /// Generate an image or video from a prompt
    /// </summary>
    Task<GenerationResult> GenerateAsync(GenerationRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get generation progress by task ID
    /// </summary>
    Task<GenerationProgress?> GetProgressAsync(string taskId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Cancel a running generation task
    /// </summary>
    Task<bool> CancelAsync(string taskId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get available models for generation
    /// </summary>
    Task<IEnumerable<GenerationModel>> GetAvailableModelsAsync(CancellationToken cancellationToken = default);
}
