using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PentestHub.API.Data.DTOs;
using PentestHub.API.Services;
using System.Security.Claims;

namespace PentestHub.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class AIController : ControllerBase
{
    private readonly IAIService _aiService;

    public AIController(IAIService aiService)
    {
        _aiService = aiService;
    }

    [HttpPost("analyze")]
    public async Task<IActionResult> Analyze([FromBody] AIAnalyzeDto analyzeDto)
    {
        var result = await _aiService.AnalyzeScanOutputAsync(analyzeDto);
        return Ok(result);
    }

    [HttpPost("chat")]
    public async Task<IActionResult> Chat([FromBody] ChatDto dto)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
        var response = await _aiService.ChatAsync(dto, userId);
        return Ok(new { response });
    }
}

