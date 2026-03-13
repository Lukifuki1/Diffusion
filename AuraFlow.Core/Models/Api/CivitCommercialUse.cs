using System.Text.Json.Serialization;

namespace AuraFlow.Core.Models.Api;

[JsonConverter(typeof(JsonStringEnumConverter<CivitCommercialUse>))]
public enum CivitCommercialUse
{
    None,
    Image,
    Rent,
    Sell
}
