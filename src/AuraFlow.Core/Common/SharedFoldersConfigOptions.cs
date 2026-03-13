using System.Text.Json;
using System.Text.Json.Serialization;

namespace AuraFlow.Core.Helper;

public class SharedFoldersConfigOptions
{
    public static SharedFoldersConfigOptions Default => new();

    public bool AlwaysWriteArray { get; set; } = false;

    public JsonSerializerOptions JsonSerializerOptions { get; set; } =
        new() { WriteIndented = true, DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault };
}
