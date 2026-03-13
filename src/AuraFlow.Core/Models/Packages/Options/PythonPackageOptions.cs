using System.Text.Json.Serialization;
using AuraFlow.Core.Python;

namespace AuraFlow.Core.Models.Packages;

public class PythonPackageOptions
{
    [JsonConverter(typeof(JsonStringEnumConverter<TorchIndex>))]
    public TorchIndex? TorchIndex { get; set; }

    public string? TorchVersion { get; set; }

    public PyVersion? PythonVersion { get; set; }
}
