namespace AuraFlow.Services;

public interface IGenerationService
{
    Task<GenerationResponse> GenerateAsync(GenerationRequest request);
    Task<bool> CancelAsync(string taskId);
}
