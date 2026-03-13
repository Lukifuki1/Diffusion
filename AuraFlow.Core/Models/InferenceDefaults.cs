using AuraFlow.Core.Models.Api.Comfy;

namespace AuraFlow.Core.Models;

public record InferenceDefaults
{
    public ComfySampler? Sampler { get; set; }
    public ComfyScheduler? Scheduler { get; set; }
    public int Steps { get; set; }
    public double CfgScale { get; set; }
    public int Width { get; set; }
    public int Height { get; set; }
}
