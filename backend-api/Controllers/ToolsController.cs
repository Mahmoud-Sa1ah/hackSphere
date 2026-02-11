using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PentestHub.API.Data.DTOs;
using PentestHub.API.Services;
using System.Security.Claims;

namespace PentestHub.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ToolsController : ControllerBase
{
    private readonly IToolService _toolService;
    private readonly ILauncherService _launcherService;
    private readonly IConfiguration _configuration;
    private readonly IDomainService _domainService;

    public ToolsController(
        IToolService toolService, 
        ILauncherService launcherService,
        IConfiguration configuration,
        IDomainService domainService)
    {
        _toolService = toolService;
        _launcherService = launcherService;
        _configuration = configuration;
        _domainService = domainService;
    }

    [HttpGet("{id}/download")]
    public async Task<IActionResult> DownloadTool(int id)
    {
        var tool = await _toolService.GetToolByIdAsync(id);
        if (tool == null)
        {
            return NotFound();
        }

        var packageData = await _toolService.GetToolPackageDataAsync(id);
        if (packageData == null || packageData.Length == 0)
        {
             return NotFound(new { message = $"Tool package for {tool.ToolName} not found in database." });
        }

        string extension = tool.PackageExtension ?? ".zip";
        string contentType = extension.ToLower() == ".pdf" ? "application/pdf" : "application/zip";
        
        return File(packageData, contentType, $"{tool.ToolName}{extension}");
    }

    [HttpGet]
    public async Task<IActionResult> GetAllTools()
    {
        var tools = await _toolService.GetAllToolsAsync();
        return Ok(tools);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetTool(int id)
    {
        var tool = await _toolService.GetToolByIdAsync(id);
        if (tool == null)
        {
            return NotFound();
        }

        return Ok(tool);
    }

    [HttpPost("{id}/upload")]
    [Authorize(Roles = "Admin")]
    [RequestSizeLimit(1073741824)] // 1GB limit
    public async Task<IActionResult> UploadTool(int id, IFormFile file)
    {
        var tool = await _toolService.GetToolByIdAsync(id);
        if (tool == null)
        {
            return NotFound();
        }

        if (file == null || file.Length == 0)
        {
            return BadRequest(new { message = "No file uploaded." });
        }

        var extension = Path.GetExtension(file.FileName).ToLower();
        if (extension != ".zip" && extension != ".pdf")
        {
            return BadRequest(new { message = "Only .zip and .pdf files are allowed." });
        }

        // Validate filename matches tool name
        var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(file.FileName);
        if (!fileNameWithoutExtension.Equals(tool.ToolName, StringComparison.OrdinalIgnoreCase))
        {
            string expected = extension == ".zip" ? $"{tool.ToolName}.zip" : $"{tool.ToolName}.pdf";
            return BadRequest(new { message = $"File name must match tool name exactly: {expected}" });
        }

        // Read file into byte array
        try 
        {
            using (var memoryStream = new MemoryStream())
            {
                await file.CopyToAsync(memoryStream);
                var packageData = memoryStream.ToArray();
                
                await _toolService.UploadToolPackageAsync(id, packageData, extension);
            }
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = $"Database Error: {ex.ToString()}" });
        }

        return Ok(new { message = $"Tool package uploaded successfully to database for {tool.ToolName}" });
    }

    [HttpPost("scan")]
    public async Task<IActionResult> CreateScan([FromBody] ScanRequestDto scanRequest)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
        
        // Domain Verification Protection for Restricted Tools
        // Example: Nmap -sS requires verified domain unless user is Admin
        if (!string.IsNullOrEmpty(scanRequest.Arguments) && scanRequest.Arguments.Contains("-sS") && scanRequest.Target != null)
        {
            if (!User.IsInRole("Admin")) 
            {
               var isVerified = await _domainService.IsDomainVerifiedAsync(userId, scanRequest.Target);
               if (!isVerified)
               {
                   return StatusCode(403, new { message = "Restricted Option Used: SYN Scan (-sS) requires domain ownership verification. Please verify this domain in Settings." });
               }
            }
        }


        try
        {
            var scan = await _toolService.CreateScanAsync(userId, scanRequest);
            // Initiate Docker execution in background
            var tool = await _toolService.GetToolByIdAsync(scanRequest.ToolId);
            if (tool != null) {
                 await _launcherService.LaunchToolAsync(scan.ScanId, userId, tool.ToolName, "", scanRequest.Arguments ?? "");
            }
            return Ok(scan);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("stop/{scanId}")]
    public async Task<IActionResult> StopScan(int scanId)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
        
        // Verify ownership
        var scan = await _toolService.GetScanByIdAsync(scanId, userId);
        if (scan == null)
        {
            return NotFound("Scan not found or access denied.");
        }

        await _launcherService.StopToolAsync(scanId);
        return Ok(new { message = "Stop signal sent." });
    }

    [HttpGet("scans")]
    public async Task<IActionResult> GetUserScans()
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
        var scans = await _toolService.GetUserScansAsync(userId);
        return Ok(scans);
    }

    [HttpGet("scans/{id}")]
    public async Task<IActionResult> GetScan(int id)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
        var scan = await _toolService.GetScanByIdAsync(id, userId);
        if (scan == null)
        {
            return NotFound();
        }
        return Ok(scan);
    }
    [HttpDelete("scans/{id}")]
    public async Task<IActionResult> DeleteScan(int id)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
        var result = await _toolService.DeleteScanAsync(id, userId);
        
        if (!result)
        {
            return NotFound();
        }
        
        return NoContent();
    }
}

