using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PentestHub.API.Services;
using System.Security.Claims;

namespace PentestHub.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class LabsController : ControllerBase
{
    private readonly ILabService _labService;

    public LabsController(ILabService labService)
    {
        _labService = labService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAllLabs()
    {
        var labs = await _labService.GetAllLabsAsync();
        return Ok(labs);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetLab(int id)
    {
        var lab = await _labService.GetLabByIdAsync(id);
        if (lab == null)
        {
            return NotFound();
        }
        return Ok(lab);
    }

    [HttpPost("{id}/submit")]
    public async Task<IActionResult> SubmitLabResult(int id, [FromBody] LabSubmissionDto dto)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
        var result = await _labService.SubmitLabResultAsync(userId, id, dto.Details);
        return Ok(result);
    }

    [HttpGet("results")]
    public async Task<IActionResult> GetUserResults()
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
        var results = await _labService.GetUserLabResultsAsync(userId);
        return Ok(results);
    }

    [HttpGet("{id}/solvers")]
    public async Task<IActionResult> GetLabSolvers(int id)
    {
        var solvers = await _labService.GetLabSolversAsync(id);
        return Ok(solvers);
    }
}

public class LabSubmissionDto
{
    public string Details { get; set; } = string.Empty;
}

