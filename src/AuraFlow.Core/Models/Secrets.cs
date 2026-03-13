using AuraFlow.Core.Models.Api.CivitApi;
using AuraFlow.Core.Models.Api.AuraCloud;

namespace AuraFlow.Core.Models;

public readonly record struct Secrets
{
    [Obsolete("Use AuraCloudAccountV2 instead")]
    public AuraCloudAccountV1Tokens? AuraCloudAccount { get; init; }

    public CivitApiTokens? CivitApi { get; init; }

    public AuraCloudAccountV2Tokens? AuraCloudAccountV2 { get; init; }

    public string? HuggingFaceToken { get; init; }
}

public static class SecretsExtensions
{
    public static bool HasLegacyAuraCloudAccount(this Secrets secrets)
    {
#pragma warning disable CS0618 // Type or member is obsolete
        return secrets.AuraCloudAccount is not null;
#pragma warning restore CS0618 // Type or member is obsolete
    }
}
