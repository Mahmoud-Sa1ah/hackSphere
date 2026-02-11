using Microsoft.EntityFrameworkCore;
using PentestHub.API.Data;
using PentestHub.API.Data.Models;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace PentestHub.API.Services;

public class ReportService : IReportService
{
    private readonly ApplicationDbContext _context;
    private readonly string _reportsPath;

    public ReportService(ApplicationDbContext context, IConfiguration configuration)
    {
        _context = context;
        _reportsPath = Path.Combine(Directory.GetCurrentDirectory(), "Reports");
        if (!Directory.Exists(_reportsPath))
        {
            Directory.CreateDirectory(_reportsPath);
        }
        QuestPDF.Settings.License = LicenseType.Community;
    }

    public async Task<Report> GenerateReportAsync(int userId, int scanId)
    {
        var scan = await _context.ScanHistory
            .Include(s => s.Tool)
            .Include(s => s.User)
            .FirstOrDefaultAsync(s => s.ScanId == scanId && s.UserId == userId);

        if (scan == null)
        {
            throw new Exception("Scan not found");
        }

        var pdfPath = Path.Combine(_reportsPath, $"report_{scanId}_{DateTime.UtcNow:yyyyMMddHHmmss}.pdf");
        
        var pdfBytes = GeneratePdfBytes(scan);
        await File.WriteAllBytesAsync(pdfPath, pdfBytes);

        var report = new Report
        {
            UserId = userId,
            ScanId = scanId,
            PdfPath = pdfPath,
            CreatedAt = DateTime.UtcNow
        };

        _context.Reports.Add(report);
        await _context.SaveChangesAsync();

        return report;
    }

    public async Task<List<Report>> GetUserReportsAsync(int userId)
    {
        return await _context.Reports
            .Include(r => r.ScanHistory!)
            .ThenInclude(s => s.Tool)
            .Where(r => r.UserId == userId)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();
    }

    public async Task<Report?> GetReportByIdAsync(int reportId, int userId)
    {
        return await _context.Reports
            .Include(r => r.ScanHistory!)
            .ThenInclude(s => s.Tool)
            .FirstOrDefaultAsync(r => r.ReportId == reportId && r.UserId == userId);
    }

    public async Task<byte[]?> GetReportPdfAsync(int reportId, int userId)
    {
        var report = await GetReportByIdAsync(reportId, userId);
        if (report == null || string.IsNullOrEmpty(report.PdfPath) || !File.Exists(report.PdfPath))
        {
            return null;
        }

        return await File.ReadAllBytesAsync(report.PdfPath);
    }

    private byte[] GeneratePdfBytes(ScanHistory scan)
    {
        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(2, Unit.Centimetre);
                page.PageColor(Colors.White);
                page.DefaultTextStyle(x => x.FontSize(10));

                page.Header()
                    .Text("PentestHub Security Report")
                    .SemiBold().FontSize(20).FontColor(Colors.Blue.Medium);

                page.Content()
                    .PaddingVertical(1, Unit.Centimetre)
                    .Column(column =>
                    {
                        column.Spacing(20);

                        column.Item().Text($"Tool: {scan.Tool?.ToolName ?? "Unknown"}").SemiBold();
                        column.Item().Text($"Target: {scan.Target}");
                        column.Item().Text($"Date: {scan.CreatedAt:yyyy-MM-dd HH:mm:ss} UTC");
                        column.Item().Text($"User: {scan.User?.Name ?? "Unknown"}");

                        if (!string.IsNullOrEmpty(scan.Arguments))
                        {
                            column.Item().Text($"Arguments: {scan.Arguments}");
                        }

                        column.Item().PaddingTop(10).Text("AI Summary").SemiBold().FontSize(14);
                        column.Item().Text(scan.AISummary ?? "No summary available");

                        column.Item().PaddingTop(10).Text("Next Steps").SemiBold().FontSize(14);
                        column.Item().Text(scan.AINextSteps ?? "No next steps available");

                        column.Item().PaddingTop(10).Text("Raw Output").SemiBold().FontSize(14);
                        column.Item().Text(scan.RawOutput ?? "No output available")
                            .FontFamily("Courier New")
                            .FontSize(8);
                    });

                page.Footer()
                    .AlignCenter()
                    .Text(x =>
                    {
                        x.Span("Page ");
                        x.CurrentPageNumber();
                        x.Span(" of ");
                        x.TotalPages();
                    });
            });
        });

        return document.GeneratePdf();
    }
    public async Task<bool> DeleteReportAsync(int reportId, int userId)
    {
        var report = await _context.Reports
            .FirstOrDefaultAsync(r => r.ReportId == reportId && r.UserId == userId);

        if (report == null)
            return false;

        // Delete the PDF file
        if (!string.IsNullOrEmpty(report.PdfPath) && File.Exists(report.PdfPath))
        {
            try
            {
                File.Delete(report.PdfPath);
            }
            catch (Exception)
            {
                // Log warning but continue with DB deletion
            }
        }

        _context.Reports.Remove(report);
        await _context.SaveChangesAsync();
        return true;
    }
}

