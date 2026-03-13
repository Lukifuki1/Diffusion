using AuraFlow.Core.Extensions;
using AuraFlow.Core.Models.Packages;
using AuraFlow.Core.Models.Progress;

namespace AuraFlow.Core.Models.PackageModification;

public class InstallPackageStep(
    BasePackage basePackage,
    string installLocation,
    InstalledPackage installedPackage,
    InstallPackageOptions options
) : ICancellablePackageStep
{
    public async Task ExecuteAsync(
        IProgress<ProgressReport>? progress = null,
        CancellationToken cancellationToken = default
    )
    {
        await basePackage
            .InstallPackage(
                installLocation,
                installedPackage,
                options,
                progress,
                progress.AsProcessOutputHandler(setMessageAsOutput: false),
                cancellationToken
            )
            .ConfigureAwait(false);
    }

    public string ProgressTitle => $"Installing {basePackage.DisplayName}...";
}
