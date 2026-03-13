using Hangfire;

namespace AuraFlow.Infrastructure.Jobs;

/// <summary>
/// Hangfire implementation of the job service
/// </summary>
public class HangfireJobService : IJobService, IDisposable
{
    private readonly JobStorageConnection _connection;
    private readonly ILogger<HangfireJobService> _logger;

    public HangfireJobService(ILogger<HangfireJobService> logger)
    {
        _logger = logger;
        
        // Get the default job storage connection
        var storage = GlobalConfiguration.Configuration.Storage as JobStorage;
        if (storage != null)
            _connection = storage.GetConnection();
    }

    public async Task<string?> EnqueueAsync<T>(string jobName, T parameters, 
        CancellationToken cancellationToken = default) where T : class
    {
        try
        {
            var jobId = BackgroundJob.Enqueue(() => ProcessJob(jobName, parameters));
            
            _logger.LogInformation("Job enqueued: {JobId} - {JobName}", jobId, jobName);
            
            return jobId;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error enqueueing job {JobName}", jobName);
            return null;
        }
    }

    public async Task<string?> AddRecurringJobAsync(string jobName, string cronExpression, 
        object? parameters = null, RecurringJobOptions? options = null)
    {
        try
        {
            var recurringId = RecurringJob.AddOrUpdate(
                jobId: jobName,
                invokeMethod: () => ProcessJob(jobName, parameters),
                cronExpression);

            _logger.LogInformation("Recurring job added: {JobId} - {Cron}", 
                recurringId, cronExpression);

            return recurringId;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding recurring job {JobName}", jobName);
            return null;
        }
    }

    public async Task RemoveJobAsync(string jobId)
    {
        try
        {
            RecurringJob.RemoveIfExists(jobId);
            
            _logger.LogInformation("Recurring job removed: {JobId}", jobId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing recurring job {JobId}", jobId);
        }
    }

    public async Task<JobStatusInfo?> GetJobStatusAsync(string jobId)
    {
        try
        {
            var history = _connection.GetJobData(jobId);
            
            if (history == null)
                return null;

            var state = ParseJobState(history.State);
            
            return new JobStatusInfo
            {
                JobId = jobId,
                JobName = history.Job?.Name ?? "Unknown",
                State = state,
                EnqueuedAt = history.EnqueuedAt,
                StartedAt = history.StartedAt,
                CompletedAt = history.CompletedAt,
                ExceptionMessage = history.Exception?.Message
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting job status for {JobId}", jobId);
            return null;
        }
    }

    private async Task ProcessJob<T>(string jobName, T parameters) where T : class
    {
        _logger.LogInformation("Processing job: {JobName} with params", jobName);
        
        // Job processing logic here
        await Task.CompletedTask;
        
        _logger.LogInformation("Completed job: {JobName}", jobName);
    }

    private static JobState ParseJobState(string? state)
    {
        return state switch
        {
            "Enqueued" => JobState.Enqueued,
            "Scheduled" => JobState.Scheduled,
            "Processing" => JobState.Processing,
            "Succeeded" => JobState.Succeeded,
            "Failed" => JobState.Failed,
            _ => JobState.Enqueued
        };
    }

    public void Dispose()
    {
        // Hangfire cleanup if needed
    }
}
