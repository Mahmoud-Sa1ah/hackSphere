using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PentestHub.API.Services;
using System.Security.Claims;

namespace PentestHub.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ReportsController : ControllerBase
{
    private readonly IReportService _reportService;

    public ReportsController(IReportService reportService)
    {
        _reportService = reportService;
    }

    [HttpPost("generate/{scanId}")]
    public async Task<IActionResult> GenerateReport(int scanId)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
        try
        {
            var report = await _reportService.GenerateReportAsync(userId, scanId);
            return Ok(report);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet]
    public async Task<IActionResult> GetUserReports()
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
        var reports = await _reportService.GetUserReportsAsync(userId);
        return Ok(reports);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetReport(int id)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
        var report = await _reportService.GetReportByIdAsync(id, userId);
        if (report == null)
        {
            return NotFound();
        }
        return Ok(report);
    }

    [HttpGet("{id}/download")]
    public async Task<IActionResult> DownloadReport(int id)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
        var pdfBytes = await _reportService.GetReportPdfAsync(id, userId);
        if (pdfBytes == null)
        {
            return NotFound();
        }
        return File(pdfBytes, "application/pdf", $"report_{id}.pdf");
    }
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteReport(int id)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
        var result = await _reportService.DeleteReportAsync(id, userId);
        
        if (!result)
        {
            return NotFound();
        }
        
        return NoContent();
    }
}

