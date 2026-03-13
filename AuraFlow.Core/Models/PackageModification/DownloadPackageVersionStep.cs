using AuraFlow.Core.Models.Packages;
using AuraFlow.Core.Models.Progress;

namespace AuraFlow.Core.Models.PackageModification;

public class DownloadPackageVersionStep(
    BasePackage package,
    string installPath,
    DownloadPackageOptions options
) : ICancellablePackageStep
{
    public Task ExecuteAsync(
        IProgress<ProgressReport>? progress = null,
        CancellationToken cancellationToken = default
    )
    {
        return package.DownloadPackage(installPath, options, progress, cancellationToken);
    }

    public string ProgressTitle => "Downloading package...";
}
