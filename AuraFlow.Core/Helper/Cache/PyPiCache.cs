using Injectio.Attributes;
using Microsoft.Extensions.Logging;
using NLog;
using Octokit;
using AuraFlow.Core.Api;
using AuraFlow.Core.Database;
using AuraFlow.Core.Models;
using AuraFlow.Core.Models.Api.Pypi;
using AuraFlow.Core.Models.Database;

namespace AuraFlow.Core.Helper.Cache;

[RegisterSingleton<IPyPiCache, PyPiCache>]
public class PyPiCache(ILiteDbContext dbContext, IPyPiApi pyPiApi, ILogger<PyPiCache> logger) : IPyPiCache
{
    private readonly TimeSpan cacheDuration = TimeSpan.FromMinutes(15);

    public async Task<IEnumerable<CustomVersion>> GetPackageVersions(string packageName)
    {
        var cacheKey = $"Pypi-{packageName}";
        var cacheEntry = await dbContext.GetPyPiCacheEntry(cacheKey).ConfigureAwait(false);
        if (cacheEntry != null && !IsCacheExpired(cacheEntry.LastUpdated))
        {
            return cacheEntry.Versions.OrderByDescending(x => x);
        }

        try
        {
            var packageInfo = await pyPiApi.GetPackageInfo(packageName).ConfigureAwait(false);
            if (packageInfo?.Releases == null)
            {
                return new List<CustomVersion>();
            }

            var newCacheEntry = new PyPiCacheEntry
            {
                CacheKey = cacheKey,
                Versions = packageInfo.Releases.Select(x => new CustomVersion(x.Key)).ToList()
            };

            await dbContext.UpsertPyPiCacheEntry(newCacheEntry).ConfigureAwait(false);

            return newCacheEntry.Versions.OrderByDescending(x => x);
        }
        catch (ApiException ex)
        {
            logger.LogWarning(ex, "Failed to get package info from PyPi API.");
            return cacheEntry?.Versions.OrderByDescending(x => x) ?? Enumerable.Empty<CustomVersion>();
        }
    }

    private bool IsCacheExpired(DateTimeOffset expiration) =>
        expiration.Add(cacheDuration) < DateTimeOffset.UtcNow;
}
