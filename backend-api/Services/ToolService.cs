using Microsoft.EntityFrameworkCore;
using PentestHub.API.Data.DTOs;
using PentestHub.API.Hubs;
using PentestHub.API.Data.Models;
using Microsoft.AspNetCore.SignalR;
using PentestHub.API.Repositories;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace PentestHub.API.Services;

public class ToolService : IToolService
{
    private readonly IToolRepository _toolRepository;
    private readonly IHubContext<LauncherHub> _launcherHub;
    private readonly ILauncherService _launcherService;

    public ToolService(
        IToolRepository toolRepository, 
        IHubContext<LauncherHub> launcherHub, 
        ILauncherService launcherService)
    {
        _toolRepository = toolRepository;
        _launcherHub = launcherHub;
        _launcherService = launcherService;
    }

    public async Task<List<Tool>> GetAllToolsAsync()
    {
        var tools = await _toolRepository.GetAllAsync();
        return tools.ToList();
    }

    public async Task<Tool?> GetToolByIdAsync(int toolId)
    {
        return await _toolRepository.GetByIdAsync(toolId);
    }

    public async Task UploadToolPackageAsync(int toolId, byte[] packageData, string packageExtension)
    {
        var tool = await _toolRepository.GetByIdAsync(toolId);
        if (tool == null)
        {
            throw new Exception("Tool not found.");
        }

        tool.PackageData = packageData;
        tool.PackageExtension = packageExtension;
        await _toolRepository.UpdateAsync(tool);
        await _toolRepository.SaveChangesAsync();
    }

    public async Task<byte[]?> GetToolPackageDataAsync(int toolId)
    {
        return await _toolRepository.GetPackageDataAsync(toolId);
    }

    public async Task<ScanHistory> CreateScanAsync(int userId, ScanRequestDto scanRequest)
    {
        var scan = new ScanHistory
        {
            UserId = userId,
            ToolId = scanRequest.ToolId,
            Target = scanRequest.Target,
            Arguments = scanRequest.Arguments,
            CreatedAt = DateTime.UtcNow
        };

        await _toolRepository.AddScanAsync(scan);
        await _toolRepository.SaveChangesAsync();

        var tool = await _toolRepository.GetByIdAsync(scanRequest.ToolId);
        if (tool != null)
        {
            await _launcherService.LaunchToolAsync(scan.ScanId, userId, tool.ToolName, "", (scanRequest.Arguments ?? "") + " " + scanRequest.Target);
        }

        return scan;
    }

    public async Task<List<ScanHistory>> GetUserScansAsync(int userId)
    {
        var scans = await _toolRepository.GetUserScansAsync(userId);
        return scans.ToList();
    }

    public async Task<ScanHistory?> GetScanByIdAsync(int scanId, int userId)
    {
        return await _toolRepository.GetScanByIdAsync(scanId, userId);
    }

    public async Task<bool> DeleteScanAsync(int scanId, int userId)
    {
        var scan = await _toolRepository.GetScanByIdAsync(scanId, userId);
        if (scan == null)
            return false;

        try
        {
            await _toolRepository.StartTransactionAsync();
            
            var reports = await _toolRepository.GetReportsByScanIdAsync(scanId);
            foreach (var report in reports)
            {
                report.ScanId = null;
            }
            await _toolRepository.SaveChangesAsync();

            await _toolRepository.DeleteScanAsync(scan);
            await _toolRepository.SaveChangesAsync();
            
            await _toolRepository.CommitTransactionAsync();
            return true;
        }
        catch
        {
            await _toolRepository.RollbackTransactionAsync();
            throw;
        }
    }
}

