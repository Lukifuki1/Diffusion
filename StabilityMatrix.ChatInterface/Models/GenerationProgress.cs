namespace StabilityMatrix.ChatInterface.Models;

public class GenerationProgress
{
    public string TaskId { get; set; } = string.Empty;
    public double Progress { get; set; }
    public GenerationStatus Status { get; set; }
    public string? CurrentStep { get; set; }
    public string? PreviewUrl { get; set; }
}

public enum GenerationStatus
{
    Pending,
    Running,
    Completed,
    Failed,
    Cancelled
}
