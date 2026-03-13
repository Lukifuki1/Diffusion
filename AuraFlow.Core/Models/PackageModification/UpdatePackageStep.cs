using AuraFlow.Core.Extensions;
using AuraFlow.Core.Models.Packages;
using AuraFlow.Core.Models.Progress;
using AuraFlow.Core.Services;

namespace AuraFlow.Core.Models.PackageModification;

public class UpdatePackageStep(
    ISettingsManager settingsManager,
    BasePackage basePackage,
    string installLocation,
    InstalledPackage installedPackage,
    UpdatePackageOptions options
) : ICancellablePackageStep
{
    public async Task ExecuteAsync(
        IProgress<ProgressReport>? progress = null,
        CancellationToken cancellationToken = default
    )
    {
        var updateResult = await basePackage
            .Update(
                installLocation,
                installedPackage,
                options,
                progress,
                progress.AsProcessOutputHandler(),
                cancellationToken
            )
            .ConfigureAwait(false);

        await using (settingsManager.BeginTransaction())
        {
            installedPackage.Version = updateResult;
            installedPackage.UpdateAvailable = false;
        }
    }

    public string ProgressTitle => $"Updating {installedPackage.DisplayName}";
}
