namespace AuraFlow.Services;

using AuraFlow.Core.Api;
using AuraFlow.Models.Api;
using AuraFlow.Infrastructure.Engines.Comfy;

public class GenerationService : IGenerationService, IDisposable
{
    private readonly ILogger<GenerationService> _logger;
    private readonly IComfyApi _comfyApi;
    private readonly IComfyWorkflowGenerator _workflowGenerator;
    private readonly IComfyWebSocketService _webSocketService;
    private readonly Dictionary<string, CancellationTokenSource> _activeGenerations = new();
    private bool _disposed;

    public GenerationService(
        ILogger<GenerationService> logger,
        IComfyApi comfyApi,
        IComfyWorkflowGenerator workflowGenerator,
        IComfyWebSocketService webSocketService)
    {
        _logger = logger;
        _comfyApi = comfyApi;
        _workflowGenerator = workflowGenerator;
        _webSocketService = webSocketService;
    }

    public async Task<GenerationResponse> GenerateAsync(GenerationRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            var response = new GenerationResponse
            {
                TaskId = Guid.NewGuid().ToString(),
                Success = true,
                IsComplete = false,
                Progress = 0
            };

            // Generate ComfyUI workflow
            var workflowJson = await _workflowGenerator.GenerateWorkflowAsync(request, cancellationToken);

            // Submit prompt to ComfyUI via API
            var promptRequest = new Core.Models.Api.Comfy.ComfyPromptRequest
            {
                ClientId = Guid.NewGuid().ToString(),
                Prompt = JsonSerializer.Deserialize<Dictionary<string, ComfyNode>>(workflowJson, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicies.SnakeCaseLower
                })!
            };

            var promptResponse = await _comfyApi.PostPrompt(promptRequest, cancellationToken);

            // Start monitoring progress via WebSocket
            response.TaskId = promptResponse.PromptId;

            // Monitor progress asynchronously
            _ = Task.Run(async () => await MonitorProgressAsync(response, request.Type, cancellationToken));

            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating {Type} with model {Model}", request.Type, request.Model);
            throw;
        }
    }

    private async Task MonitorProgressAsync(GenerationResponse response, string type, CancellationToken cancellationToken)
    {
        try
        {
            // Subscribe to WebSocket events
            _webSocketService.OnProgressUpdate += (sender, data) =>
            {
                var progressData = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(data);
                if (progressData != null && progressData.TryGetValue("value", out var value))
                {
                    response.Progress = value.GetInt32();
                }
            };

            _webSocketService.OnRunningNodeChanged += (sender, nodeId) =>
            {
                Console.WriteLine($"Executing: {nodeId}");
            };

            _webSocketService.OnExecutionComplete += (sender, data) =>
            {
                response.IsComplete = true;
            };

            // Subscribe and send prompt
            await _webSocketService.SubscribeToQueueAsync();

            try
            {
                while (!response.IsComplete && !cancellationToken.IsCancellationRequested)
                {
                    await Task.Delay(500, cancellationToken);

                    if (response.Progress >= 100)
                    {
                        response.IsComplete = true;
                        response.ResultUrl = $"/output/{response.TaskId}.{type.ToLower()}";
                    }
                }
            }
            finally
            {
                await _webSocketService.UnsubscribeFromQueueAsync();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error monitoring progress for task {TaskId}", response.TaskId);
        }
    }

    public async Task<bool> CancelAsync(string taskId, CancellationToken cancellationToken = default)
    {
        if (_activeGenerations.TryGetValue(taskId, out var cts))
        {
            cts.Cancel();
            _activeGenerations.Remove(taskId);

            // Also interrupt via ComfyUI API
            try
            {
                await _comfyApi.PostInterrupt(cancellationToken);
            }
            catch
            {
                // Ignore interruption errors
            }

            return true;
        }
        return false;
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

            _webSocketService?.Dispose();
            _disposed = true;
        }
    }
}
