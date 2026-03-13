using AuraFlow.Core.Models.Packages;

namespace AuraFlow.Core.Models;


/// <summary>
/// Pair of InstalledPackage and BasePackage
/// </summary>
public record PackagePair(InstalledPackage InstalledPackage, BasePackage BasePackage);
