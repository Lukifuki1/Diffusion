using Microsoft.AspNetCore.Mvc;
using AuraFlow.Core.Services;

namespace AuraFlow.Api.Controllers;

/// <summary>
/// REST API controller for generation tracking and management
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class GenerationsController : ControllerBase
{
    private readonly IGenerationService _generationService;

    public GenerationsController(IGenerationService generationService)
    {
        _generationService = generationService;
    }

    /// <summary>
    /// Generate an image
    /// </summary>
    [HttpPost("image")]
    public async Task<ActionResult<GenerationResult>> GenerateImage([FromBody] GenerationRequest request)
    {
        try
        {
            var result = await _generationService.GenerateImageAsync(request);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Generate a video
    /// </summary>
    [HttpPost("video")]
    public async Task<ActionResult<GenerationResult>> GenerateVideo([FromBody] GenerationRequest request)
    {
        try
        {
            var result = await _generationService.GenerateVideoAsync(request);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Get generation progress by task ID
    /// </summary>
    [HttpGet("{taskId}/progress")]
    public async Task<ActionResult<GenerationProgress>> GetProgress(string taskId)
    {
        var progress = await _generationService.GetProgressAsync(taskId);
        
        if (progress == null)
            return NotFound();
            
        return Ok(progress);
    }

    /// <summary>
    /// Cancel a running generation
    /// </summary>
    [HttpPost("{taskId}/cancel")]
    public async Task<ActionResult> CancelGeneration(string taskId)
    {
        try
        {
            await _generationService.CancelGenerationAsync(taskId);
            return Ok(new { message = "Generation cancelled" });
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Get all generations with filtering
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<GenerationResult>>> GetAllGenerations(
        [FromQuery] string? status = null,
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null)
    {
        var generations = await _generationService.GetGenerationsAsync(status, startDate, endDate);
        return Ok(generations);
    }

    /// <summary>
    /// Get generation by ID
    /// </summary>
    [HttpGet("{taskId}")]
    public async Task<ActionResult<GenerationResult>> GetGeneration(string taskId)
    {
        var generation = await _generationService.GetGenerationByIdAsync(taskId);
        
        if (generation == null)
            return NotFound();
            
        return Ok(generation);
    }

    /// <summary>
    /// Delete old generations to free up space
    /// </summary>
    [HttpDelete]
    public async Task<ActionResult> DeleteOldGenerations(
        [FromQuery] int daysToKeep = 30,
        [FromQuery] bool deleteImages = true)
    {
        try
        {
            var deletedCount = await _generationService.CleanupOldGenerationsAsync(daysToKeep, deleteImages);
            return Ok(new { deletedCount });
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Get batch of generations for pagination
    /// </summary>
    [HttpGet("batch")]
    public async Task<ActionResult<PagedResult<GenerationResult>>> GetBatch(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        var result = await _generationService.GetGenerationsBatchAsync(page, pageSize);
        return Ok(result);
    }
}

/// <summary>
/// Request model for generation
/// </summary>
public record GenerationRequest(
    string Prompt,
    string? NegativePrompt,
    string ModelId,
    int Width,
    int Height,
    int Steps,
    float GuidanceScale,
    long Seed,
    string? OutputFormat = "png"
);

/// <summary>
/// Response model for generation result
/// </summary>
public record GenerationResult(
    string TaskId,
    bool Success,
    string Status,
    string? ImageUrl,
    string? VideoUrl,
    DateTime CreatedAt,
    double DurationMs
);

/// <summary>
/// Response model for generation progress
/// </summary>
public record GenerationProgress(
    string TaskId,
    bool IsComplete,
    double ProgressPercentage,
    string StatusMessage,
    string? CurrentStep
);

/// <summary>
/// Paged result wrapper
/// </summary>
public record PagedResult<T>(
    IEnumerable<T> Items,
    int TotalCount,
    int Page,
    int PageSize
);
