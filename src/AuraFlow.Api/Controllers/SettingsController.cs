using Microsoft.AspNetCore.Mvc;
using AuraFlow.Core.Services;

namespace AuraFlow.Api.Controllers;

/// <summary>
/// REST API controller for application settings and configuration
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class SettingsController : ControllerBase
{
    private readonly ISettingsManager _settingsManager;

    public SettingsController(ISettingsManager settingsManager)
    {
        _settingsManager = settingsManager;
    }

    /// <summary>
    /// Get all settings
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<Dictionary<string, object>>> GetAllSettings()
    {
        var settings = await _settingsManager.GetAllSettingsAsync();
        return Ok(settings);
    }

    /// <summary>
    /// Get setting by key
    /// </summary>
    [HttpGet("{key}")]
    public async Task<ActionResult<object>> GetSetting(string key)
    {
        var value = await _settingsManager.GetSettingAsync(key);
        
        if (value == null)
            return NotFound();
            
        return Ok(value);
    }

    /// <summary>
    /// Set or update a setting
    /// </summary>
    [HttpPost("{key}")]
    public async Task<ActionResult> SetSetting(string key, [FromBody] object value)
    {
        try
        {
            await _settingsManager.SetSettingAsync(key, value);
            return NoContent();
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Update multiple settings at once
    /// </summary>
    [HttpPut]
    public async Task<ActionResult> UpdateSettings([FromBody] Dictionary<string, object> settings)
    {
        try
        {
            foreach (var kvp in settings)
            {
                await _settingsManager.SetSettingAsync(kvp.Key, kvp.Value);
            }
            return NoContent();
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Delete a setting by key
    /// </summary>
    [HttpDelete("{key}")]
    public async Task<ActionResult> DeleteSetting(string key)
    {
        try
        {
            await _settingsManager.DeleteSettingAsync(key);
            return NoContent();
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Get application info and version
    /// </summary>
    [HttpGet("info")]
    public async Task<ActionResult<ApplicationInfo>> GetApplicationInfo()
    {
        var info = await _settingsManager.GetApplicationInfoAsync();
        return Ok(info);
    }

    /// <summary>
    /// Reset all settings to defaults
    /// </summary>
    [HttpPost("reset")]
    public async Task<ActionResult> ResetSettings()
    {
        try
        {
            await _settingsManager.ResetToDefaultsAsync();
            return Ok(new { message = "Settings reset to defaults" });
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Export settings to JSON
    /// </summary>
    [HttpGet("export")]
    public async Task<ActionResult<string>> ExportSettings()
    {
        var json = await _settingsManager.ExportSettingsAsync();
        return Ok(json);
    }

    /// <summary>
    /// Import settings from JSON
    /// </summary>
    [HttpPost("import")]
    public async Task<ActionResult> ImportSettings([FromBody] string json)
    {
        try
        {
            await _settingsManager.ImportSettingsAsync(json);
            return Ok(new { message = "Settings imported successfully" });
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
}

/// <summary>
/// Application info response model
/// </summary>
public record ApplicationInfo(
    string Version,
    string BuildDate,
    string Platform,
    Dictionary<string, object> Features
);
