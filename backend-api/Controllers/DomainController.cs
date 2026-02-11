using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PentestHub.API.Data.Models;
using PentestHub.API.Services;
using System.Security.Claims;

namespace PentestHub.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class DomainController : ControllerBase
{
    private readonly IDomainService _domainService;

    public DomainController(IDomainService domainService)
    {
        _domainService = domainService;
    }

    [HttpPost("upload")]
    [RequestSizeLimit(10485760)] // 10MB limit
    public async Task<IActionResult> UploadProof([FromForm] string domain, IFormFile file)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");

        if (string.IsNullOrWhiteSpace(domain))
            return BadRequest(new { message = "Domain name is required." });

        if (file == null || file.Length == 0)
            return BadRequest(new { message = "Proof file is required." });

        try
        {
            var result = await _domainService.UploadProofAsync(userId, domain, file);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("status")]
    public async Task<IActionResult> GetStatus()
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
        var verifications = await _domainService.GetUserVerificationsAsync(userId);
        return Ok(verifications);
    }
}
