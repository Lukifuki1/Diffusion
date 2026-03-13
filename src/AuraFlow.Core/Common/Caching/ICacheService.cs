namespace AuraFlow.Core.Common.Caching;

/// <summary>
/// Cache service interface for consistent caching across the application
/// </summary>
public interface ICacheService
{
    /// <summary>
    /// Get value from cache
    /// </summary>
    Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Set value in cache with optional expiration
    /// </summary>
    Task SetAsync<T>(string key, T value, TimeSpan? expiration = null, 
        CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Remove value from cache
    /// </summary>
    Task RemoveAsync(string key, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Check if key exists in cache
    /// </summary>
    Task<bool> ExistsAsync(string key, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Clear all cache entries (useful for testing or maintenance)
    /// </summary>
    Task ClearAsync(CancellationToken cancellationToken = default);
}
