using System.Security.Claims;
using OpenIddict.Abstractions;
using AuraFlow.Core.Api;
using AuraFlow.Core.Api.AuraCloudAuthApi;

namespace AuraFlow.Core.Models.Api.AuraCloud;

public class AuraCloudAccountStatusUpdateEventArgs : EventArgs
{
    public static AuraCloudAccountStatusUpdateEventArgs Disconnected { get; } = new();

    public bool IsConnected { get; init; }

    public ClaimsPrincipal? Principal { get; init; }

    public AccountResponse? User { get; init; }

    public string? Id => Principal?.GetClaim(OpenIddictConstants.Claims.Subject);

    public string? DisplayName =>
        Principal?.GetClaim(OpenIddictConstants.Claims.PreferredUsername) ?? Principal?.Identity?.Name;

    public string? Email => Principal?.GetClaim(OpenIddictConstants.Claims.Email);

    public bool IsPatreonConnected => User?.PatreonId != null;

    public bool IsActiveSupporter => User?.Roles.Contains("ActivePatron") == true;
}
