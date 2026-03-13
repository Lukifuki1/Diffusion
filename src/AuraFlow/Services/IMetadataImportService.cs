using AuraFlow.Core.Models;
using AuraFlow.Core.Models.FileInterfaces;
using AuraFlow.Core.Models.Progress;

namespace AuraFlow.Core.Services;

public interface IMetadataImportService
{
    Task ScanDirectoryForMissingInfo(DirectoryPath directory, IProgress<ProgressReport>? progress = null);

    Task<ConnectedModelInfo?> GetMetadataForFile(
        FilePath filePath,
        IProgress<ProgressReport>? progress = null,
        bool forceReimport = false
    );

    Task UpdateExistingMetadata(DirectoryPath directory, IProgress<ProgressReport>? progress = null);
}
