using Microsoft.AspNetCore.Mvc;
using AuraFlow.Core.Services;

namespace AuraFlow.Api.Controllers;

/// <summary>
/// REST API controller for package management
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class PackagesController : ControllerBase
{
    private readonly IPackageService _packageService;

    public PackagesController(IPackageService packageService)
    {
        _packageService = packageService;
    }

    /// <summary>
    /// Get all available packages
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Package>>> GetAllPackages()
    {
        var packages = await _packageService.GetAllPackagesAsync();
        return Ok(packages);
    }

    /// <summary>
    /// Get package by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<Package>> GetPackage(string id)
    {
        var package = await _packageService.GetPackageByIdAsync(id);
        
        if (package == null)
            return NotFound();
            
        return Ok(package);
    }

    /// <summary>
    /// Install a new package
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<Package>> InstallPackage([FromBody] InstallPackageRequest request)
    {
        try
        {
            var package = await _packageService.InstallPackageAsync(request);
            return CreatedAtAction(nameof(GetPackage), new { id = package.Id }, package);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Update an existing package
    /// </summary>
    [HttpPut("{id}")]
    public async Task<ActionResult<Package>> UpdatePackage(string id, [FromBody] InstallPackageRequest request)
    {
        try
        {
            var package = await _packageService.UpdatePackageAsync(id, request);
            return Ok(package);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Delete a package
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<ActionResult> DeletePackage(string id)
    {
        try
        {
            await _packageService.DeletePackageAsync(id);
            return NoContent();
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Get package versions
    /// </summary>
    [HttpGet("{id}/versions")]
    public async Task<ActionResult<IEnumerable<PackageVersion>>> GetPackageVersions(string id)
    {
        var versions = await _packageService.GetPackageVersionsAsync(id);
        return Ok(versions);
    }
}

/// <summary>
/// Request model for installing/updating packages
/// </summary>
public record InstallPackageRequest(
    string PackageId,
    string? Version = null,
    Dictionary<string, string>? Options = null
);
