using System.Text.Json.Serialization;
using AuraFlow.Core.Converters.Json;

namespace AuraFlow.Core.Models.Api;

[JsonConverter(typeof(DefaultUnknownEnumConverter<CivitModelFormat>))]
public enum CivitModelFormat
{
    Unknown,
    SafeTensor,
    PickleTensor,
    Diffusers,
    GGUF,
    Other
}
