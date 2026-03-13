using AuraFlow.Core.Inference;
using AuraFlow.Core.Services;
using Microsoft.Extensions.Logging;

namespace AuraFlow.Services;

/// <summary>
/// Implementation of the generation service that handles image and video generation using ComfyUI
/// </summary>
public class GenerationService : IGenerationService, IDisposable
{
    private readonly ILogger<GenerationService> _logger;
    private readonly ComfyClient _comfyClient;
    private readonly Dictionary<string, CancellationTokenSource> _activeTasks = new();
    private bool _isDisposed;

    public GenerationService(ILogger<GenerationService> logger, ComfyClient comfyClient)
    {
        _logger = logger;
        _comfyClient = comfyClient;
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
            _activeTasks[request.Prompt] = cts;

            // Build ComfyUI workflow based on model and type
            var workflow = await BuildWorkflowAsync(request, cancellationToken);
            
            // Queue the prompt to ComfyUI
            var task = await _comfyClient.QueuePromptAsync(workflow, cts.Token);
            
            // Wait for completion with progress updates
            await WaitForCompletionAsync(task, request, stopwatch, cts.Token);

            return new GenerationResult
            {
                TaskId = task.Id,
                Success = true,
                OutputFiles = GetOutputFiles(task),
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
        if (!_comfyClient.PromptTasks.TryGetValue(taskId, out var task))
        {
            return null;
        }

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

    /// <summary>
    /// Cancel a running generation task
    /// </summary>
    public async Task<bool> CancelAsync(string taskId, CancellationToken cancellationToken = default)
    {
        try
        {
            await _comfyClient.InterruptPromptAsync(cancellationToken);
            
            if (_activeTasks.TryGetValue(taskId, out var cts))
            {
                cts.Cancel();
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
        var modelNames = await _comfyClient.GetModelNamesAsync(cancellationToken);
        
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

    private async Task<Dictionary<string, ComfyNode>> BuildWorkflowAsync(
        GenerationRequest request, CancellationToken cancellationToken)
    {
        // TODO: Implement workflow building based on model type and parameters
        // This will create the appropriate ComfyUI node graph for the requested generation
        
        var nodes = new Dictionary<string, ComfyNode>();
        
        // Add checkpoint loader node
        var checkpointLoader = new CheckpointLoaderSimpleNode
        {
            CkptName = request.ModelName,
            LoadDevice = "auto"
        };
        nodes["checkpoint_loader"] = checkpointLoader;

        // Add KSampler node for image generation
        if (request.Type == GenerationType.Image)
        {
            var kSampler = new KSamplerNode
            {
                Seed = request.Options.Seed,
                Steps = request.Options.Steps,
                CFG = (float)request.Options.GuidanceScale,
                SamplerName = "euler",
                Scheduler = "normal",
                Denoise = 1.0
            };
            nodes["k_sampler"] = kSampler;
        }

        // Add CLIP Text Encode nodes for positive/negative prompts
        var clipTextEncodePositive = new ClipTextEncodeNode
        {
            Clip = "clip_loader_output",
            Text = request.Prompt
        };
        nodes["clip_text_encode_positive"] = clipTextEncodePositive;

        return nodes;
    }

    private async Task WaitForCompletionAsync(
        ComfyTask task, 
        GenerationRequest request,
        System.Diagnostics.Stopwatch stopwatch,
        CancellationToken cancellationToken)
    {
        // Wait for the task to complete with timeout
        var timeout = TimeSpan.FromSeconds(request.Options.Steps * 10);
        
        try
        {
            await task.Task.WaitAsync(timeout, cancellationToken);
        }
        catch (TimeoutException)
        {
            _logger.LogWarning("Generation timed out after {Timeout}s", timeout.TotalSeconds);
            throw;
        }
    }

    private IEnumerable<string> GetOutputFiles(ComfyTask task)
    {
        // TODO: Extract output file paths from the completed task
        // This will depend on how ComfyUI stores generated images/videos
        
        return new List<string>();
    }

    public void Dispose()
    {
        if (_isDisposed) return;

        foreach (var cts in _activeTasks.Values)
        {
            cts.Cancel();
            cts.Dispose();
        }

        _comfyClient?.Dispose();
        _isDisposed = true;
    }
}
