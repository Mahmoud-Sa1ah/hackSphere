using PentestHub.API.Data.Models;

namespace PentestHub.API.Repositories;

public interface IToolRepository
{
    Task<IEnumerable<Tool>> GetAllAsync();
    Task<Tool?> GetByIdAsync(int id);
    Task<byte[]?> GetPackageDataAsync(int toolId);
    Task UpdateAsync(Tool tool);
    Task SaveChangesAsync();
    
    // Scan related (could be moved to IScanRepository later, but for now following current logic)
    Task AddScanAsync(ScanHistory scan);
    Task<IEnumerable<ScanHistory>> GetUserScansAsync(int userId);
    Task<ScanHistory?> GetScanByIdAsync(int scanId, int userId);
    Task DeleteScanAsync(ScanHistory scan);
    Task<IEnumerable<Report>> GetReportsByScanIdAsync(int scanId);
    Task StartTransactionAsync();
    Task CommitTransactionAsync();
    Task RollbackTransactionAsync();
}
