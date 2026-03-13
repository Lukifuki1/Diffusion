using AuraFlow.Models.Api;

namespace AuraFlow.Infrastructure.Engines.Comfy;

public interface IComfyWorkflowGenerator
{
    Task<string> GenerateWorkflowAsync(GenerationRequest request, CancellationToken cancellationToken = default);
}
