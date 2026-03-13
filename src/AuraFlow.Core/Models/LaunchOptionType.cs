using System.Text.Json.Serialization;

namespace AuraFlow.Core.Models;

[JsonConverter(typeof(JsonStringEnumConverter<LaunchOptionType>))]
public enum LaunchOptionType
{
    Bool,
    String,
    Int
}
