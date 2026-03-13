using AuraFlow.ChatInterface.Models;
using Websocket.Client;

namespace AuraFlow.ChatInterface.Services;

public class ChatInterfaceClient : IDisposable
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<ChatInterfaceClient> _logger;
    private Websocket.Client.IWebsocketClient? _websocketClient;
    private bool _disposed;

    public ChatInterfaceClient(HttpClient httpClient, ILogger<ChatInterfaceClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<GenerationResponse> GenerateAsync(
        string prompt, 
        GenerationOptions options,
        CancellationToken cancellationToken = default)
    {
        var request = new GenerationRequest
        {
            Prompt = prompt,
            Type = GenerationType.Image,
            Options = options
        };

        var response = await _httpClient.PostAsJsonAsync("/api/v1/generate", request, cancellationToken);
        
        if (!response.IsSuccessStatusCode)
        {
            return new GenerationResponse 
            { 
                Success = false, 
                ErrorMessage = $"API error: {(int)response.StatusCode}" 
            };
        }

        var result = await response.Content.ReadFromJsonAsync<GenerationResponse>(cancellationToken);
        return result ?? new GenerationResponse { Success = false, ErrorMessage = "Unknown error" };
    }

    public async Task<GenerationProgress> GetProgressAsync(string taskId, CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.GetAsync($"/api/v1/progress/{taskId}", cancellationToken);
        
        if (!response.IsSuccessStatusCode)
        {
            return new GenerationProgress 
            { 
                TaskId = taskId, 
                Status = GenerationStatus.Failed,
                Progress = 0
            };
        }

        var progress = await response.Content.ReadFromJsonAsync<GenerationProgress>(cancellationToken);
        return progress ?? new GenerationProgress { TaskId = taskId, Status = GenerationStatus.Failed };
    }

    public async Task StartWebSocketAsync(string baseUrl)
    {
        _websocketClient = new Websocket.Client(new Uri(baseUrl));
        
        _websocketClient.MessageReceived.Subscribe(msg =>
        {
            var progress = System.Text.Json.JsonSerializer.Deserialize<GenerationProgress>(msg.Text);
            _logger.LogInformation("WebSocket message received: {Status}", progress?.Status);
        });

        await _websocketClient.Start();
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            _websocketClient?.Dispose();
            _disposed = true;
        }
    }
}
