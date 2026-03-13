namespace StabilityMatrix.ChatInterface.Models;

public class ChatMessage
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Content { get; set; } = string.Empty;
    public MessageType Type { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public GenerationStatus Status { get; set; }
}

public enum MessageType
{
    User,
    System,
    Assistant
}
