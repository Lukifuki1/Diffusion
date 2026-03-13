using System.Text.Json.Serialization;

namespace AuraFlow.Core.Models;

[JsonDerivedType(typeof(CivitPostDownloadContextAction), "CivitPostDownload")]
[JsonDerivedType(typeof(ModelPostDownloadContextAction), "ModelPostDownload")]
public interface IContextAction
{
    object? Context { get; set; }
}
