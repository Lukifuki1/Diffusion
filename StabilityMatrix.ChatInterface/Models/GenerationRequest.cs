namespace StabilityMatrix.ChatInterface.Models;

public class GenerationRequest
{
    public string Prompt { get; set; } = string.Empty;
    public string ModelName { get; set; } = string.Empty;
    public GenerationType Type { get; set; }
    public GenerationOptions Options { get; set; } = new();
}

public class GenerationOptions
{
    public int Width { get; set; } = 1024;
    public int Height { get; set; } = 1024;
    public int Steps { get; set; } = 30;
    public double GuidanceScale { get; set; } = 7.5;
    public long Seed { get; set; } = -1; // -1 means random
}

public enum GenerationType
{
    Image,
    Video
}
