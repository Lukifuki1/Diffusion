using AuraFlow.Core.Api;
using AuraFlow.Core.Inference;
using AuraFlow.Core.Services;
using AuraFlow.Infrastructure.Engines.Comfy;
using Microsoft.Extensions.Logging;

namespace AuraFlow.Services;

/// <summary>
/// Implementation of the generation service that handles image and video generation using ComfyUI
/// </summary>
public class GenerationService : IGenerationService, IDisposable
{
    private readonly ILogger<GenerationService> _logger;
    private readonly IComfyApi _comfyApi;
    private readonly IComfyWorkflowGenerator _workflowGenerator;
    private readonly ComfyClient _comfyClient;
    private readonly Dictionary<string, CancellationTokenSource> _activeGenerations = new();
    private bool _disposed;

    public GenerationService(
        ILogger<GenerationService> logger,
        IComfyApi comfyApi,
        IComfyWorkflowGenerator workflowGenerator,
        ComfyClient? comfyClient = null)
    {
        _logger = logger;
        _comfyApi = comfyApi;
        _workflowGenerator = workflowGenerator;
        _comfyClient = comfyClient ?? new ComfyClient("http://comfyui:8188");
    }

    /// <summary>
    /// Generate an image or video from a prompt using ComfyUI workflow
    /// </summary>
    public async Task<GenerationResult> GenerateAsync(GenerationRequest request, CancellationToken cancellationToken = default)
    {
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        
        try
        {
            _logger.LogInformation("Starting generation for model: {ModelName}, type: {Type}", 
                request.ModelName, request.Type);

            // Create cancellation token source for this task
            var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            _activeGenerations[request.Prompt] = cts;

            // Generate ComfyUI workflow
            var workflowJson = await _workflowGenerator.GenerateWorkflowAsync(request, cancellationToken);
            
            // Submit prompt to ComfyUI
            var promptRequest = new Core.Models.Api.Comfy.ComfyPromptRequest
            {
                ClientId = Guid.NewGuid().ToString(),
                Prompt = JsonSerializer.Deserialize<Dictionary<string, ComfyNode>>(workflowJson, new JsonSerializerOptions 
                { 
                    PropertyNamingPolicy = JsonNamingPolicies.SnakeCaseLower 
                })!
            };

            var promptResponse = await _comfyApi.PostPrompt(promptRequest, cancellationToken);
            
            return new GenerationResult
            {
                TaskId = promptResponse.PromptId,
                Success = true,
                OutputFiles = Enumerable.Empty<string>(),
                DurationMs = stopwatch.ElapsedMilliseconds,
                ModelName = request.ModelName,
                Type = request.Type,
                CompletedAt = DateTime.UtcNow
            };
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("Generation cancelled for prompt: {Prompt}", request.Prompt);
            return new GenerationResult
            {
                TaskId = Guid.NewGuid().ToString(),
                Success = false,
                ErrorMessage = "Generation was cancelled",
                ModelName = request.ModelName,
                Type = request.Type,
                CompletedAt = DateTime.UtcNow
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating content for model: {ModelName}", request.ModelName);
            return new GenerationResult
            {
                TaskId = Guid.NewGuid().ToString(),
                Success = false,
                ErrorMessage = ex.Message,
                ModelName = request.ModelName,
                Type = request.Type,
                CompletedAt = DateTime.UtcNow
            };
        }
    }

    /// <summary>
    /// Get generation progress by task ID
    /// </summary>
    public async Task<GenerationProgress?> GetProgressAsync(string taskId, CancellationToken cancellationToken = default)
    {
        if (_comfyClient != null && _comfyClient.PromptTasks.TryGetValue(taskId, out var task))
        {
            return new GenerationProgress
            {
                TaskId = taskId,
                Status = task.IsCompleted 
                    ? GenerationStatus.Completed 
                    : GenerationStatus.Running,
                ProgressPercent = task.ProgressPercentage,
                CurrentStep = task.CurrentStep,
                TotalSteps = task.TotalSteps,
                CurrentNode = task.RunningNode?.NodeId,
                StartedAt = task.CreatedAt
            };
        }

        return new GenerationProgress
        {
            TaskId = taskId,
            Status = GenerationStatus.Pending,
            ProgressPercent = 0
        };
    }

    /// <summary>
    /// Cancel a running generation task
    /// </summary>
    public async Task<bool> CancelAsync(string taskId, CancellationToken cancellationToken = default)
    {
        try
        {
            await _comfyApi.PostInterrupt(cancellationToken);
            
            if (_activeGenerations.TryGetValue(taskId, out var cts))
            {
                cts.Cancel();
                _activeGenerations.Remove(taskId);
                return true;
            }

            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error cancelling task: {TaskId}", taskId);
            return false;
        }
    }

    /// <summary>
    /// Get available models for generation
    /// </summary>
    public async Task<IEnumerable<GenerationModel>> GetAvailableModelsAsync(CancellationToken cancellationToken = default)
    {
        return new[]
        {
            new GenerationModel 
            { 
                Id = "flux-dev", 
                Name = "Flux Dev", 
                Type = GenerationType.Image,
                Description = "High-quality image generation model"
            },
            new GenerationModel 
            { 
                Id = "sdxl-turbo", 
                Name = "SDXL Turbo", 
                Type = GenerationType.Image,
                Description = "Fast image generation with SDXL architecture"
            },
            new GenerationModel 
            { 
                Id = "wan2gp", 
                Name = "Wan2GP", 
                Type = GenerationType.Video,
                Description = "Video generation model"
            }
        };
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            foreach (var cts in _activeGenerations.Values)
            {
                cts.Cancel();
                cts.Dispose();
            }
            _comfyClient?.Dispose();
            _disposed = true;
        }
    }
}
