using Injectio.Attributes;
using OpenIddict.Client;
using AuraFlow.Core.Api.AuraCloudAuthApi;
using AuraFlow.Core.Attributes;
using AuraFlow.Core.Models.Api.AuraCloud;
using AuraFlow.Core.Services;

namespace AuraFlow.Core.Api;

[RegisterSingleton<AuraCloudAuthTokenProvider>]
public class AuraCloudAuthTokenProvider(
    Lazy<IAuraCloudAuthApiV2> lazyAuraCloudAuthApi,
    ISecretsManager secretsManager,
    OpenIddictClientService openIdClient
) : ITokenProvider
{
    private readonly Lazy<IAuraCloudAuthApiV2> lazyAuraCloudAuthApi = lazyAuraCloudAuthApi;

    // Lazy as instantiating requires the current class to be instantiated.

    /// <inheritdoc />
    public async Task<string> GetAccessTokenAsync()
    {
        var secrets = await secretsManager.SafeLoadAsync().ConfigureAwait(false);

        return secrets.AuraCloudAccountV2?.AccessToken ?? "";
    }

    /// <inheritdoc />
    public async Task<(string AccessToken, string RefreshToken)> RefreshTokensAsync()
    {
        var secrets = await secretsManager.SafeLoadAsync().ConfigureAwait(false);

        if (string.IsNullOrWhiteSpace(secrets.AuraCloudAccountV2?.RefreshToken))
        {
            throw new InvalidOperationException("No refresh token found");
        }

        var result = await openIdClient
            .AuthenticateWithRefreshTokenAsync(
                new OpenIddictClientModels.RefreshTokenAuthenticationRequest
                {
                    ProviderName = OpenIdClientConstants.AuraCloudAccount.ProviderName,
                    RefreshToken = secrets.AuraCloudAccountV2.RefreshToken
                }
            )
            .ConfigureAwait(false);

        if (string.IsNullOrEmpty(result.RefreshToken))
        {
            throw new InvalidOperationException("No refresh token returned");
        }

        secrets = secrets with
        {
            AuraCloudAccountV2 = new AuraCloudAccountV2Tokens(
                result.AccessToken,
                result.RefreshToken,
                result.IdentityToken
            )
        };

        await secretsManager.SaveAsync(secrets).ConfigureAwait(false);

        return (result.AccessToken, result.RefreshToken);
    }
}
