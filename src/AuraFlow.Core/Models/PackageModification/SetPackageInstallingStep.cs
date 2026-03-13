using AuraFlow.Core.Models.Progress;
using AuraFlow.Core.Services;

namespace AuraFlow.Core.Models.PackageModification;

public class SetPackageInstallingStep : IPackageStep
{
    private readonly ISettingsManager settingsManager;
    private readonly string packageName;

    public SetPackageInstallingStep(ISettingsManager settingsManager, string packageName)
    {
        this.settingsManager = settingsManager;
        this.packageName = packageName;
    }

    public Task ExecuteAsync(IProgress<ProgressReport>? progress = null)
    {
        settingsManager.PackageInstallsInProgress.Add(packageName);
        return Task.CompletedTask;
    }

    public string ProgressTitle => "Starting Package Installation";
}
