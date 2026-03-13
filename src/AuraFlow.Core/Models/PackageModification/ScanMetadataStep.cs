using AuraFlow.Core.Models.FileInterfaces;
using AuraFlow.Core.Models.Progress;
using AuraFlow.Core.Services;

namespace AuraFlow.Core.Models.PackageModification;

public class ScanMetadataStep(
    DirectoryPath directoryPath,
    IMetadataImportService metadataImportService,
    bool updateExistingMetadata = false
) : IPackageStep
{
    public Task ExecuteAsync(IProgress<ProgressReport>? progress = null) =>
        updateExistingMetadata
            ? metadataImportService.UpdateExistingMetadata(directoryPath, progress)
            : metadataImportService.ScanDirectoryForMissingInfo(directoryPath, progress);

    public string ProgressTitle => "Updating Metadata";
}
