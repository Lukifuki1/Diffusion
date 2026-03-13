namespace AuraFlow.Models.Api;

public class GenerationResponse
{
    public string TaskId { get; set; } = "";
    public bool Success { get; set; }
    public bool IsComplete { get; set; }
    public int Progress { get; set; }
    public string ResultUrl { get; set; } = "";
    public string PreviewUrl { get; set; } = "";
}
