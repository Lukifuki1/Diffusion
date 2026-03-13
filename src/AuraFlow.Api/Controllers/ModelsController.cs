using Microsoft.AspNetCore.Mvc;
using AuraFlow.Core.Services;

namespace AuraFlow.Api.Controllers;

/// <summary>
/// REST API controller for model management
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class ModelsController : ControllerBase
{
    private readonly IModelService _modelService;

    public ModelsController(IModelService modelService)
    {
        _modelService = modelService;
    }

    /// <summary>
    /// Get all available models with filtering
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Model>>> GetAllModels(
        [FromQuery] string? category = null,
        [FromQuery] string? search = null,
        [FromQuery] string? baseModel = null)
    {
        var models = await _modelService.GetModelsAsync(category, search, baseModel);
        return Ok(models);
    }

    /// <summary>
    /// Get model by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<Model>> GetModel(string id)
    {
        var model = await _modelService.GetModelByIdAsync(id);
        
        if (model == null)
            return NotFound();
            
        return Ok(model);
    }

    /// <summary>
    /// Download a model from Civitai or other sources
    /// </summary>
    [HttpPost("{id}/download")]
    public async Task<ActionResult<DownloadStatus>> DownloadModel(string id, [FromBody] DownloadRequest request)
    {
        try
        {
            var status = await _modelService.DownloadModelAsync(id, request);
            return Ok(status);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Get model versions
    /// </summary>
    [HttpGet("{id}/versions")]
    public async Task<ActionResult<IEnumerable<ModelVersion>>> GetModelVersions(string id)
    {
        var versions = await _modelService.GetModelVersionsAsync(id);
        return Ok(versions);
    }

    /// <summary>
    /// Delete a model from local storage
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteModel(string id)
    {
        try
        {
            await _modelService.DeleteModelAsync(id);
            return NoContent();
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Get installed models
    /// </summary>
    [HttpGet("installed")]
    public async Task<ActionResult<IEnumerable<Model>>> GetInstalledModels()
    {
        var models = await _modelService.GetInstalledModelsAsync();
        return Ok(models);
    }
}

/// <summary>
/// Request model for downloading models
/// </summary>
public record DownloadRequest(
    string VersionId,
    string? TargetPath = null
);

/// <summary>
/// Response model for download status
/// </summary>
public record DownloadStatus(
    string TaskId,
    bool IsDownloading,
    double ProgressPercentage,
    string StatusMessage
);
