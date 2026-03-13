namespace AuraFlow.Core.Models.Api.AuraCloud;

public record GetUserResponse
{
    public required string Id { get; init; }
    public required AuraCloudAccount Account { get; init; }
    public required HashSet<AuraCloudRole> UserRoles { get; init; }
    public string? PatreonId { get; init; }
    public bool IsEmailVerified { get; init; }
    public bool CanHasDevBuild { get; init; }
    public bool CanHasPreviewBuild { get; init; }

    public bool IsActiveSupporter => CanHasDevBuild || CanHasPreviewBuild;
}
