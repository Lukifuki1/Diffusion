using AuraFlow.Core.Extensions;

namespace AuraFlow.Core.Models.Inference;

public enum ModelLoader
{
    [StringValue("Default")]
    Default,

    [StringValue("GGUF")]
    Gguf,

    [StringValue("nf4")]
    Nf4,

    [StringValue("UNet")]
    Unet
}
