using StabilityMatrix.ChatInterface.Models;

namespace StabilityMatrix.ChatInterface.Services;

public class GenerationService : IDisposable
{
    private readonly ChatInterfaceClient _client;
    private readonly ILogger<GenerationService> _logger;
    private readonly Dictionary<string, CancellationTokenSource> _activeGenerations = new();
    private bool _disposed;

    public GenerationService(ChatInterfaceClient client, ILogger<GenerationService> logger)
    {
        _client = client;
        _logger = logger;
    }

    public async Task<GenerationResponse> GenerateImageAsync(
        string prompt, 
        ImageOptions options,
        CancellationToken cancellationToken = default)
    {
        var generationOptions = new GenerationOptions
        {
            Width = options.Width,
            Height = options.Height,
            Steps = options.Steps,
            GuidanceScale = options.GuidanceScale,
            Seed = options.Seed
        };

        var response = await _client.GenerateAsync(prompt, generationOptions, cancellationToken);
        
        if (response.Success)
        {
            _activeGenerations[response.TaskId] = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            
            // Monitor progress in background
            _ = Task.Run(async () => await MonitorProgress(response.TaskId, _activeGenerations[response.TaskId].Token));
        }

        return response;
    }

    public async Task<GenerationResponse> GenerateVideoAsync(
        string prompt, 
        VideoOptions options,
        CancellationToken cancellationToken = default)
    {
        var generationOptions = new GenerationOptions
        {
            Width = options.Width,
            Height = options.Height,
            Steps = options.Frames,
            GuidanceScale = options.GuidanceScale,
            Seed = options.Seed
        };

        return await _client.GenerateAsync(prompt, generationOptions, cancellationToken);
    }

    public async Task<GenerationProgress> GetProgressAsync(string taskId)
    {
        return await _client.GetProgressAsync(taskId);
    }

    public async Task CancelGenerationAsync(string taskId)
    {
        if (_activeGenerations.TryGetValue(taskId, out var cts))
        {
            cts.Cancel();
            _logger.LogInformation("Cancelled generation: {TaskId}", taskId);
        }
    }

    private async Task MonitorProgress(string taskId, CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                var progress = await _client.GetProgressAsync(taskId, cancellationToken);
                
                if (progress.Status == GenerationStatus.Completed || 
                    progress.Status == GenerationStatus.Failed)
                {
                    break;
                }

                await Task.Delay(1000, cancellationToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
        }
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
            _activeGenerations.Clear();
            _client?.Dispose();
            _disposed = true;
        }
    }
}

public class ImageOptions
{
    public int Width { get; set; } = 1024;
    public int Height { get; set; } = 1024;
    public int Steps { get; set; } = 30;
    public double GuidanceScale { get; set; } = 7.5;
    public long Seed { get; set; } = -1;
}

public class VideoOptions
{
    public int Width { get; set; } = 512;
    public int Height { get; set; } = 512;
    public int Frames { get; set; } = 24;
    public double GuidanceScale { get; set; } = 7.5;
    public long Seed { get; set; } = -1;
}
