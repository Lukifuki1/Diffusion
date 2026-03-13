namespace AuraFlow.Services;

public interface IComfyWebSocketService : IDisposable
{
    event EventHandler<string>? OnProgressUpdate;
    event EventHandler<string>? OnRunningNodeChanged;
    event EventHandler<string>? OnExecutionComplete;
    event EventHandler<string>? OnExecutionError;
    
    Task ConnectAsync(string baseUrl, CancellationToken cancellationToken = default);
    Task DisconnectAsync(CancellationToken cancellationToken = default);
    Task SubscribeToQueueAsync();
    Task UnsubscribeFromQueueAsync();
    Task SendPromptAsync(string promptJson, string clientId, CancellationToken cancellationToken = default);
}
