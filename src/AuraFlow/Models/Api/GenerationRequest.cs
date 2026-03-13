namespace AuraFlow.Models.Api;

public class GenerationRequest
{
    public string Prompt { get; set; } = "";
    public string Model { get; set; } = "Flux Dev";
    public string Type { get; set; } = "Image";
    public int Width { get; set; } = 1024;
    public int Height { get; set; } = 1024;
    public int Steps { get; set; } = 30;
    public float GuidanceScale { get; set; } = 7.5f;
}
