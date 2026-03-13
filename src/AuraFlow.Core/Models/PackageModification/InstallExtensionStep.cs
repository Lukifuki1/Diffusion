using AuraFlow.Core.Models.Packages.Extensions;
using AuraFlow.Core.Models.Progress;

namespace AuraFlow.Core.Models.PackageModification;

public class InstallExtensionStep(
    IPackageExtensionManager extensionManager,
    InstalledPackage installedPackage,
    PackageExtension packageExtension,
    PackageExtensionVersion? extensionVersion = null
) : IPackageStep
{
    public Task ExecuteAsync(IProgress<ProgressReport>? progress = null)
    {
        return extensionManager.InstallExtensionAsync(
            packageExtension,
            installedPackage,
            extensionVersion,
            progress
        );
    }

    public string ProgressTitle => $"Installing Extension {packageExtension.Title}";
}
