using AuraFlow.Core.Models;

namespace AuraFlow.Core.Helper.Cache;

public interface IPyPiCache
{
    Task<IEnumerable<CustomVersion>> GetPackageVersions(string packageName);
}
