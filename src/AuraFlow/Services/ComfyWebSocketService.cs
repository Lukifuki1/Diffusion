using System.Text.Json;
using Websocket.Client;

namespace AuraFlow.Services;

public class ComfyWebSocketService : IComfyWebSocketService
{
    private readonly ILogger<ComfyWebSocketService> _logger;
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicies.SnakeCaseLower,
        WriteIndented = true
    };

    private WebsocketClient? _webSocketClient;
    private bool _isConnected;
    private string? _clientId;
    private bool _disposed;

    public event EventHandler<string>? OnProgressUpdate;
    public event EventHandler<string>? OnRunningNodeChanged;
    public event EventHandler<string>? OnExecutionComplete;
    public event EventHandler<string>? OnExecutionError;

    public ComfyWebSocketService(ILogger<ComfyWebSocketService> logger)
    {
        _logger = logger;
    }

    public async Task ConnectAsync(string baseUrl, CancellationToken cancellationToken = default)
    {
        try
        {
            var uri = new Uri(baseUrl.Replace("http", "ws"));
            
            _webSocketClient = new WebsocketClient(uri);
            _webSocketClient.MessageReceived += async e => await HandleMessageAsync(e);
            _webSocketClient.ConnectionStateChanged += state =>
            {
                _isConnected = state == WebsocketConnectionState.Open;
                return Task.CompletedTask;
            };

            await _webSocketClient.Start();
            _logger.LogInformation("WebSocket connected to ComfyUI");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to connect WebSocket to ComfyUI");
            throw;
        }
    }

    public async Task DisconnectAsync(CancellationToken cancellationToken = default)
    {
        if (_webSocketClient != null && _isConnected)
        {
            await _webSocketClient.Stop();
            _logger.LogInformation("WebSocket disconnected from ComfyUI");
        }
    }

    public async Task SubscribeToQueueAsync()
    {
        if (_clientId == null || _webSocketClient == null)
            return;

        var subscribeMessage = new Dictionary<string, object>
        {
            ["cmd"] = "subscribe",
            ["prompt_id"] = "*" // Subscribe to all prompts
        };

        await SendJsonAsync(subscribeMessage);
    }

    public async Task UnsubscribeFromQueueAsync()
    {
        if (_clientId == null || _webSocketClient == null)
            return;

        var unsubscribeMessage = new Dictionary<string, object>
        {
            ["cmd"] = "unsubscribe",
            ["prompt_id"] = "*"
        };

        await SendJsonAsync(unsubscribeMessage);
    }

    public async Task SendPromptAsync(string promptJson, string clientId, CancellationToken cancellationToken = default)
    {
        _clientId = clientId;

        var message = new Dictionary<string, object>
        {
            ["cmd"] = "prompt",
            ["client_id"] = clientId,
            ["prompt"] = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(promptJson, _jsonOptions)!
        };

        await SendJsonAsync(message);
    }

    private async Task HandleMessageAsync(WebsocketMessage e)
    {
        if (e is TextMessage textMessage)
        {
            try
            {
                var message = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(textMessage.Text!);
                
                if (message != null && message.TryGetValue("type", out var type))
                {
                    switch (type.GetString())
                    {
                        case "progress":
                            var progressData = JsonSerializer.Serialize(message["data"]!);
                            OnProgressUpdate?.Invoke(this, progressData);
                            break;

                        case "executing":
                            var executingData = message["data"];
                            if (executingData.TryGetProperty("node", out var node))
                            {
                                OnRunningNodeChanged?.Invoke(this, node.GetString() ?? "");
                            }
                            else if (executingData.TryGetProperty("id", out var id) && id.GetString() == null)
                            {
                                // Execution complete
                                OnExecutionComplete?.Invoke(this, message["data"]!.GetRawText());
                            }
                            break;

                        case "execution_error":
                            var errorData = JsonSerializer.Serialize(message["data"]!);
                            OnExecutionError?.Invoke(this, errorData);
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error handling WebSocket message");
            }
        }
    }

    private async Task SendJsonAsync(Dictionary<string, object> message)
    {
        if (_webSocketClient != null && _isConnected)
        {
            var json = JsonSerializer.Serialize(message, _jsonOptions);
            await _webSocketClient.Send(json);
        }
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            _webSocketClient?.Dispose();
            _disposed = true;
        }
    }
}
