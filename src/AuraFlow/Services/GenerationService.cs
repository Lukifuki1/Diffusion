namespace AuraFlow.Services;

public class GenerationService : IGenerationService, IDisposable
{
    private readonly ILogger<GenerationService> _logger;
    private readonly Dictionary<string, CancellationTokenSource> _activeGenerations = new();
    private bool _disposed;

    public GenerationService(ILogger<GenerationService> logger)
    {
        _logger = logger;
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

            // Simulate generation process (in real implementation, this would call ComfyUI)
            await Task.Run(async () =>
            {
                for (int i = 0; i <= 100; i += 10)
                {
                    response.Progress = i;
                    await Task.Delay(500, cancellationToken);
                    
                    if (i == 100)
                    {
                        response.IsComplete = true;
                        response.ResultUrl = $"/output/{response.TaskId}.{request.Type.ToLower()}";
                    }
                }
            }, cancellationToken);

            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating {Type} with model {Model}", request.Type, request.Model);
            throw;
        }
    }

    public async Task<bool> CancelAsync(string taskId, CancellationToken cancellationToken = default)
    {
        if (_activeGenerations.TryGetValue(taskId, out var cts))
        {
            cts.Cancel();
            _activeGenerations.Remove(taskId);
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
            _disposed = true;
        }
    }
}
