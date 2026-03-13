using System.Text.Json.Serialization;

namespace AuraFlow.Core.Models.Api;

[JsonConverter(typeof(JsonStringEnumConverter<CivitMode>))]
public enum CivitMode
{
    Archived,
    TakenDown
}
