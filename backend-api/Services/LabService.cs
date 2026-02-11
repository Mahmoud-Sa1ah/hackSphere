using Microsoft.EntityFrameworkCore;
using PentestHub.API.Data.Models;
using PentestHub.API.Repositories;

namespace PentestHub.API.Services;

public class LabService : ILabService
{
    private readonly ILabRepository _labRepository;
    private readonly IUserRepository _userRepository;
    private readonly IAIService _aiService;

    public LabService(ILabRepository labRepository, IUserRepository userRepository, IAIService aiService)
    {
        _labRepository = labRepository;
        _userRepository = userRepository;
        _aiService = aiService;
    }

    public async Task<List<Lab>> GetAllLabsAsync()
    {
        var labs = await _labRepository.GetAllAsync();
        return labs.ToList();
    }

    public async Task<Lab?> GetLabByIdAsync(int labId)
    {
        return await _labRepository.GetByIdAsync(labId);
    }

    public async Task<LabResult> SubmitLabResultAsync(int userId, int labId, string details)
    {
        // Check if user has already solved this lab
        var existingResult = await _labRepository.GetUserLabResultAsync(userId, labId);

        if (existingResult != null)
        {
            return existingResult;
        }

        var lab = await _labRepository.GetByIdAsync(labId);
        var result = new LabResult
        {
            LabId = labId,
            UserId = userId,
            Details = details,
            Score = 100,
            CompletionTime = DateTime.UtcNow
        };

        await _labRepository.AddLabResultAsync(result);

        // GAMIFICATION LOGIC
        var user = await _userRepository.GetByIdAsync(userId);
        if (user != null && lab != null)
        {
            user.Points += lab.Points;
            user.CompletedRooms++;

            var today = DateTime.UtcNow.Date;
            if (user.LastLabSolvedDate.HasValue)
            {
                var lastSolved = user.LastLabSolvedDate.Value.Date;
                if (lastSolved == today.AddDays(-1))
                {
                    user.Streak++;
                }
                else if (lastSolved < today.AddDays(-1))
                {
                    user.Streak = 1;
                }
            }
            else
            {
                user.Streak = 1;
            }
            user.LastLabSolvedDate = DateTime.UtcNow;

            await _userRepository.UpdateAsync(user);

            // Award Badges
            var badges = await _labRepository.GetAllBadgesAsync();
            var userBadgeIds = await _labRepository.GetUserBadgeIdsAsync(userId);

            foreach (var badge in badges)
            {
                if (user.Points >= badge.PointsRequired && !userBadgeIds.Contains(badge.BadgeId))
                {
                    await _labRepository.AddUserBadgeAsync(new UserBadge
                    {
                        UserId = userId,
                        BadgeId = badge.BadgeId,
                        DateEarned = DateTime.UtcNow
                    });
                }
            }
        }

        await _labRepository.SaveChangesAsync();
        await _userRepository.SaveChangesAsync();

        // Generate AI feedback separately
        if (lab != null)
        {
            try
            {
                var aiFeedback = await _aiService.GenerateLabFeedbackAsync(details, lab.Title, lab.Description ?? "");
                result.AIFeedback = aiFeedback;
                // Since result is tracked by labRepository, we can just save again
                await _labRepository.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"AI Feedback failed: {ex.Message}");
            }
        }

        return result;
    }

    public async Task<List<LabResult>> GetUserLabResultsAsync(int userId)
    {
        var results = await _labRepository.GetUserResultsAsync(userId);
        return results.ToList();
    }

    public async Task<List<object>> GetLabSolversAsync(int labId)
    {
        var results = await _labRepository.GetLabSolversAsync(labId);
        var solvers = results
            .Select(r => new
            {
                r.UserId,
                Name = r.User != null ? r.User.Name : "Unknown",
                ProfilePhoto = r.User != null ? r.User.ProfilePhoto : null,
                DateSolved = r.CompletionTime
            })
            .ToList();

        // Group in memory to ensure uniqueness per user and take the first solve
        var uniqueSolvers = solvers
            .GroupBy(s => s.UserId)
            .Select(g => (object)g.First())
            .ToList();

        return uniqueSolvers;
    }
}

