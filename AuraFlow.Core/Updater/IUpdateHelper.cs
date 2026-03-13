using AuraFlow.Core.Models.Progress;
using AuraFlow.Core.Models.Update;

namespace AuraFlow.Core.Updater;

public interface IUpdateHelper
{
    event EventHandler<UpdateStatusChangedEventArgs>? UpdateStatusChanged;

    Task StartCheckingForUpdates();

    Task CheckForUpdate();

    Task DownloadUpdate(UpdateInfo updateInfo, IProgress<ProgressReport> progress);
}
