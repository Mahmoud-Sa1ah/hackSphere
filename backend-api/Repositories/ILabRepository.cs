using PentestHub.API.Data.Models;

namespace PentestHub.API.Repositories;

public interface ILabRepository
{
    Task<IEnumerable<Lab>> GetAllAsync();
    Task<Lab?> GetByIdAsync(int id);
    Task<LabResult?> GetUserLabResultAsync(int userId, int labId);
    Task AddLabResultAsync(LabResult result);
    Task<IEnumerable<LabResult>> GetUserResultsAsync(int userId);
    Task<IEnumerable<LabResult>> GetLabSolversAsync(int labId);
    Task<IEnumerable<Badge>> GetAllBadgesAsync();
    Task<IEnumerable<int>> GetUserBadgeIdsAsync(int userId);
    Task AddUserBadgeAsync(UserBadge userBadge);
    Task SaveChangesAsync();
}
