using DynamicData.Binding;
using AuraFlow.Core.Models;
using AuraFlow.Core.Models.Database;
using AuraFlow.Core.Models.FileInterfaces;

namespace AuraFlow.Core.Services;

public interface IImageIndexService
{
    IndexCollection<LocalImageFile, string> InferenceImages { get; }

    /// <summary>
    /// Refresh index for all collections
    /// </summary>
    Task RefreshIndexForAllCollections();

    Task RefreshIndex(IndexCollection<LocalImageFile, string> indexCollection);

    /// <summary>
    /// Refreshes the index of local images in the background
    /// </summary>
    void BackgroundRefreshIndex();
}
