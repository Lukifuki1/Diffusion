using AuraFlow.Core.Processes;

namespace AuraFlow.Core.Models.Packages;

public class RunPackageOptions
{
    public string? Command { get; set; }

    public ProcessArgs Arguments { get; set; } = [];
}
