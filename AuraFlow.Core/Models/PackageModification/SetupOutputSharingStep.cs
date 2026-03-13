using AuraFlow.Core.Models.Packages;
using AuraFlow.Core.Models.Progress;

namespace AuraFlow.Core.Models.PackageModification;

public class SetupOutputSharingStep(BasePackage package, string installPath) : IPackageStep
{
    public Task ExecuteAsync(IProgress<ProgressReport>? progress = null)
    {
        package.SetupOutputFolderLinks(installPath);
        return Task.CompletedTask;
    }

    public string ProgressTitle => "Setting up output sharing...";
}
