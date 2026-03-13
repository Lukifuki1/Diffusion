using AuraFlow.Core.Models.Packages.Extensions;
using AuraFlow.Core.Models.Progress;

namespace AuraFlow.Core.Models.PackageModification;

public class UninstallExtensionStep(
    IPackageExtensionManager extensionManager,
    InstalledPackage installedPackage,
    InstalledPackageExtension packageExtension
) : IPackageStep
{
    public Task ExecuteAsync(IProgress<ProgressReport>? progress = null)
    {
        return extensionManager.UninstallExtensionAsync(packageExtension, installedPackage, progress);
    }

    public string ProgressTitle => $"Uninstalling Extension {packageExtension.Title}";
}
