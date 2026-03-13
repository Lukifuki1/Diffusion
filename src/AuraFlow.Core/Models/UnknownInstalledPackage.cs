using AuraFlow.Core.Models.Packages;
using AuraFlow.Core.Python;

namespace AuraFlow.Core.Models;

public class UnknownInstalledPackage : InstalledPackage
{
    public static UnknownInstalledPackage FromDirectoryName(string name)
    {
        return new UnknownInstalledPackage
        {
            Id = Guid.NewGuid(),
            PackageName = UnknownPackage.Key,
            DisplayName = name,
            PythonVersion = PyInstallationManager.Python_3_10_17.StringValue,
            LibraryPath = $"Packages{System.IO.Path.DirectorySeparatorChar}{name}",
        };
    }
}
