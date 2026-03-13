using AuraFlow.Services;
using Microsoft.AspNetCore.Mvc;

namespace AuraFlow.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class GenerationController : ControllerBase
{
    private readonly IGenerationService _generationService;
    private readonly ILogger<GenerationController> _logger;

    public GenerationController(IGenerationService generationService, ILogger<GenerationController> logger)
    {
        _generationService = generationService;
        _logger = logger;
    }

    /// <summary>
    /// Generate an image or video from a prompt
    /// </summary>
    [HttpPost("generate")]
    public async Task<IActionResult> Generate([FromBody] GenerationRequest request)
    {
        try
        {
            var result = await _generationService.GenerateAsync(request);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating content for model: {ModelName}", request.ModelName);
            return StatusCode(500, new { message = "Generation failed", error = ex.Message });
        }
    }

    /// <summary>
    /// Get generation progress by task ID
    /// </summary>
    [HttpGet("progress/{taskId}")]
    public async Task<IActionResult> GetProgress(string taskId)
    {
        try
        {
            var progress = await _generationService.GetProgressAsync(taskId);
            return Ok(progress);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting progress for task: {TaskId}", taskId);
            return StatusCode(500, new { message = "Progress retrieval failed", error = ex.Message });
        }
    }

    /// <summary>
    /// Get available models
    /// </summary>
    [HttpGet("models")]
    public IActionResult GetModels()
    {
        var models = new[]
        {
            new { id = "flux-dev", name = "Flux Dev", type = "photo" },
            new { id = "sdxl-turbo", name = "SDXL Turbo", type = "photo" },
            new { id = "realistic-vision", name = "Realistic Vision", type = "photo" },
            new { id = "wan2gp", name = "Wan2GP", type = "video" },
            new { id = "cogvideo", name = "CogVideo", type = "video" }
        };
        return Ok(models);
    }
}
