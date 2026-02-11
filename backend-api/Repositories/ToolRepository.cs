using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using PentestHub.API.Data;
using PentestHub.API.Data.Models;

namespace PentestHub.API.Repositories;

public class ToolRepository : IToolRepository
{
    private readonly ApplicationDbContext _context;
    private IDbContextTransaction? _transaction;

    public ToolRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Tool>> GetAllAsync()
    {
        return await _context.Tools
            .Select(t => new Tool
            {
                ToolId = t.ToolId,
                ToolName = t.ToolName,
                Description = t.Description,
                Category = t.Category,
                BinaryName = t.BinaryName,
                RequiredExtensions = t.RequiredExtensions,
                DownloadUrl = t.DownloadUrl,
                PackageExtension = t.PackageExtension
            })
            .ToListAsync();
    }

    public async Task<Tool?> GetByIdAsync(int id)
    {
        return await _context.Tools
            .Where(t => t.ToolId == id)
            .Select(t => new Tool
            {
                ToolId = t.ToolId,
                ToolName = t.ToolName,
                Description = t.Description,
                Category = t.Category,
                BinaryName = t.BinaryName,
                RequiredExtensions = t.RequiredExtensions,
                DownloadUrl = t.DownloadUrl,
                PackageExtension = t.PackageExtension
            })
            .FirstOrDefaultAsync();
    }

    public async Task<byte[]?> GetPackageDataAsync(int toolId)
    {
        return await _context.Tools
            .Where(t => t.ToolId == toolId)
            .Select(t => t.PackageData)
            .FirstOrDefaultAsync();
    }

    public async Task UpdateAsync(Tool tool)
    {
        _context.Tools.Attach(tool);
        _context.Entry(tool).Property(x => x.PackageData).IsModified = true;
        _context.Entry(tool).Property(x => x.PackageExtension).IsModified = true;
        await Task.CompletedTask;
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }

    public async Task AddScanAsync(ScanHistory scan)
    {
        await _context.ScanHistory.AddAsync(scan);
    }

    public async Task<IEnumerable<ScanHistory>> GetUserScansAsync(int userId)
    {
        return await _context.ScanHistory
            .Include(s => s.Tool)
            .Where(s => s.UserId == userId)
            .OrderByDescending(s => s.CreatedAt)
            .ToListAsync();
    }

    public async Task<ScanHistory?> GetScanByIdAsync(int scanId, int userId)
    {
        return await _context.ScanHistory
            .Include(s => s.Tool)
            .FirstOrDefaultAsync(s => s.ScanId == scanId && s.UserId == userId);
    }

    public async Task DeleteScanAsync(ScanHistory scan)
    {
        _context.ScanHistory.Remove(scan);
        await Task.CompletedTask;
    }

    public async Task<IEnumerable<Report>> GetReportsByScanIdAsync(int scanId)
    {
        return await _context.Reports
            .Where(r => r.ScanId == scanId)
            .ToListAsync();
    }

    public async Task StartTransactionAsync()
    {
        _transaction = await _context.Database.BeginTransactionAsync();
    }

    public async Task CommitTransactionAsync()
    {
        if (_transaction != null)
        {
            await _transaction.CommitAsync();
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    public async Task RollbackTransactionAsync()
    {
        if (_transaction != null)
        {
            await _transaction.RollbackAsync();
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }
}
