using System.Text.Json.Serialization;
using AuraFlow.Core.Converters.Json;

namespace AuraFlow.Core.Models.Api.Comfy;

public enum ComfyUpscalerType
{
    None,
    Latent,
    ESRGAN,
    DownloadableModel
}
