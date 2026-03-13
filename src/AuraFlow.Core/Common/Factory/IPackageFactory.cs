using AuraFlow.Core.Models;
using AuraFlow.Core.Models.Packages;

namespace AuraFlow.Core.Helper.Factory;

public interface IPackageFactory
{
    IEnumerable<BasePackage> GetAllAvailablePackages();
    BasePackage? FindPackageByName(string? packageName);
    BasePackage? this[string packageName] { get; }
    PackagePair? GetPackagePair(InstalledPackage? installedPackage);
    IEnumerable<BasePackage> GetPackagesByType(PackageType packageType);
    BasePackage GetNewBasePackage(InstalledPackage installedPackage);
}
