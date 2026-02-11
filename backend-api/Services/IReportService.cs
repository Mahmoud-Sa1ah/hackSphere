using PentestHub.API.Data.Models;

namespace PentestHub.API.Services;

public interface IReportService
{
    Task<Report> GenerateReportAsync(int userId, int scanId);
    Task<List<Report>> GetUserReportsAsync(int userId);
    Task<Report?> GetReportByIdAsync(int reportId, int userId);
    Task<byte[]?> GetReportPdfAsync(int reportId, int userId);
    Task<bool> DeleteReportAsync(int reportId, int userId);
}

