using AuraFlow.Core.Models.Progress;

namespace AuraFlow.Core.Models.PackageModification;

public interface IPackageStep
{
    Task ExecuteAsync(IProgress<ProgressReport>? progress = null);
    string ProgressTitle { get; }
}
