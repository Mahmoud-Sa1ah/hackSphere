using PentestHub.API.Data.Models;

namespace PentestHub.API.Services;

public interface ILabService
{
    Task<List<Lab>> GetAllLabsAsync();
    Task<Lab?> GetLabByIdAsync(int labId);
    Task<LabResult> SubmitLabResultAsync(int userId, int labId, string details);
    Task<List<LabResult>> GetUserLabResultsAsync(int userId);
    Task<List<object>> GetLabSolversAsync(int labId);
}
