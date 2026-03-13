namespace AuraFlow.Core.Models.Api.AuraCloud;

public record GetFilesDownloadResponse
{
    public required Uri DownloadUrl { get; set; }

    public DateTimeOffset? ExpiresAt { get; set; }
}
