using AuraFlow.Core.Models.FileInterfaces;
using AuraFlow.Core.Models.Packages;

namespace AuraFlow.Core.Helper;

public interface ISharedFolders
{
    void SetupLinksForPackage(BasePackage basePackage, DirectoryPath installDirectory);
    void RemoveLinksForAllPackages();
}
