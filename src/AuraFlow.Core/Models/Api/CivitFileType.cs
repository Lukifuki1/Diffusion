using System.Runtime.Serialization;
using System.Text.Json.Serialization;
using AuraFlow.Core.Converters.Json;

namespace AuraFlow.Core.Models.Api;

[JsonConverter(typeof(DefaultUnknownEnumConverter<CivitFileType>))]
public enum CivitFileType
{
    Unknown,
    Model,
    VAE,
    Config,
    Archive,

    [EnumMember(Value = "Pruned Model")]
    PrunedModel,

    [EnumMember(Value = "Training Data")]
    TrainingData
}
