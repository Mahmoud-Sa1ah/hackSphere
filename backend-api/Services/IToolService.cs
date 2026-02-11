using PentestHub.API.Data.DTOs;
using PentestHub.API.Data.Models;

namespace PentestHub.API.Services;

public interface IToolService
{
    Task<List<Tool>> GetAllToolsAsync();
    Task<Tool?> GetToolByIdAsync(int toolId);
    Task<ScanHistory> CreateScanAsync(int userId, ScanRequestDto scanRequest);
    Task<List<ScanHistory>> GetUserScansAsync(int userId);
    Task<ScanHistory?> GetScanByIdAsync(int scanId, int userId);
    Task UploadToolPackageAsync(int toolId, byte[] packageData, string packageExtension);
    Task<byte[]?> GetToolPackageDataAsync(int toolId);
    Task<bool> DeleteScanAsync(int scanId, int userId);
}
