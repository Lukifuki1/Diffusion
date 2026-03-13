namespace AuraFlow.Infrastructure.Jobs;

/// <summary>
/// Background job service interface for scheduled and recurring tasks
/// </summary>
public interface IJobService
{
    /// <summary>
    /// Schedule a one-time job
    /// </summary>
    Task<string?> EnqueueAsync<T>(string jobName, T parameters, 
        CancellationToken cancellationToken = default) where T : class;
    
    /// <summary>
    /// Schedule a recurring job
    /// </summary>
    Task<string?> AddRecurringJobAsync(string jobName, string cronExpression, 
        object? parameters = null, RecurringJobOptions? options = null);
    
    /// <summary>
    /// Remove a scheduled job
    /// </summary>
    Task RemoveJobAsync(string jobId);
    
    /// <summary>
    /// Get job status and history
    /// </summary>
    Task<JobStatusInfo?> GetJobStatusAsync(string jobId);
}

/// <summary>
/// Job status information
/// </summary>
public class JobStatusInfo
{
    public string? JobId { get; set; }
    public string? JobName { get; set; }
    public JobState State { get; set; }
    public DateTime? EnqueuedAt { get; set; }
    public DateTime? StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public string? ExceptionMessage { get; set; }
}

/// <summary>
/// Job execution state
/// </summary>
public enum JobState
{
    Enqueued,
    Scheduled,
    Processing,
    Succeeded,
    Failed,
    Retrying
}

/// <summary>
/// Recurring job options
/// </summary>
public class RecurringJobOptions
{
    public string? Timezone { get; set; }
    public bool UseUtcSchedule { get; set; } = true;
}
