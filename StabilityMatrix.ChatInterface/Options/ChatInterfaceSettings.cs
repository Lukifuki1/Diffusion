namespace StabilityMatrix.ChatInterface.Options;

public class ChatInterfaceSettings
{
    public const string SectionName = "ChatInterface";
    
    public bool Enabled { get; set; } = true;
    public string DefaultModel { get; set; } = "Flux Dev";
    public int MaxConcurrentGenerations { get; set; } = 3;
    public int TimeoutSeconds { get; set; } = 120;
    public string ApiBaseUrl { get; set; } = "http://localhost:5000";
}
