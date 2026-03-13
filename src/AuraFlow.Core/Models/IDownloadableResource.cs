using System.Diagnostics.CodeAnalysis;

namespace AuraFlow.Core.Models;

/// <summary>
/// Interface for items that may have a downloadable resource.
/// </summary>
public interface IDownloadableResource
{
    /// <summary>
    /// Downloadable resource information.
    /// </summary>
    RemoteResource? DownloadableResource { get; }

    [MemberNotNullWhen(true, nameof(DownloadableResource))]
    bool IsDownloadable => DownloadableResource is not null;
}
