using AuraFlow.Core.Helper;
using AuraFlow.Core.Models.Packages;
using AuraFlow.Core.Models.Progress;
using AuraFlow.Core.Python;

namespace AuraFlow.Core.Models.PackageModification;

public class SetupPrerequisitesStep(
    IPrerequisiteHelper prerequisiteHelper,
    BasePackage package,
    PyVersion? pythonVersion = null
) : IPackageStep
{
    public async Task ExecuteAsync(IProgress<ProgressReport>? progress = null)
    {
        await prerequisiteHelper
            .InstallPackageRequirements(package, pythonVersion, progress)
            .ConfigureAwait(false);
    }

    public string ProgressTitle => "Installing prerequisites...";
}
