using Microsoft.EntityFrameworkCore;
using PentestHub.API.Data;
using PentestHub.API.Data.Models;

namespace PentestHub.API.Repositories;

public class LabRepository : ILabRepository
{
    private readonly ApplicationDbContext _context;

    public LabRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Lab>> GetAllAsync()
    {
        return await _context.Labs.ToListAsync();
    }

    public async Task<Lab?> GetByIdAsync(int id)
    {
        return await _context.Labs.FindAsync(id);
    }

    public async Task<LabResult?> GetUserLabResultAsync(int userId, int labId)
    {
        return await _context.LabResults
            .FirstOrDefaultAsync(r => r.UserId == userId && r.LabId == labId && r.Score >= 100);
    }

    public async Task AddLabResultAsync(LabResult result)
    {
        await _context.LabResults.AddAsync(result);
    }

    public async Task<IEnumerable<LabResult>> GetUserResultsAsync(int userId)
    {
        return await _context.LabResults
            .Include(r => r.Lab)
            .Where(r => r.UserId == userId)
            .OrderByDescending(r => r.CompletionTime)
            .ToListAsync();
    }

    public async Task<IEnumerable<LabResult>> GetLabSolversAsync(int labId)
    {
        return await _context.LabResults
            .Include(r => r.User)
            .Where(r => r.LabId == labId && r.Score >= 100)
            .OrderBy(r => r.CompletionTime)
            .ToListAsync();
    }

    public async Task<IEnumerable<Badge>> GetAllBadgesAsync()
    {
        return await _context.Badges.ToListAsync();
    }

    public async Task<IEnumerable<int>> GetUserBadgeIdsAsync(int userId)
    {
        return await _context.UserBadges
            .Where(ub => ub.UserId == userId)
            .Select(ub => ub.BadgeId)
            .ToListAsync();
    }

    public async Task AddUserBadgeAsync(UserBadge userBadge)
    {
        await _context.UserBadges.AddAsync(userBadge);
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}
