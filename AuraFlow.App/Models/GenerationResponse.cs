namespace AuraFlow.ChatInterface.Models;

public class GenerationResponse
{
    public string TaskId { get; set; } = Guid.NewGuid().ToString();
    public bool Success { get; set; }
    public string? Url { get; set; }
    public string? ErrorMessage { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
