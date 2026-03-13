using System.Text.Json.Serialization;
using AuraFlow.Core.Converters.Json;

namespace AuraFlow.Core.Models.Update;

[JsonConverter(typeof(DefaultUnknownEnumConverter<UpdateChannel>))]
public enum UpdateChannel
{
    Unknown,
    Stable,
    Preview,
    Development
}
