using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PentestHub.API.Data;
using PentestHub.API.Data.Models;
using System.Security.Claims;

namespace PentestHub.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class GamificationController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public GamificationController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet("leaderboard")]
    public async Task<IActionResult> GetLeaderboard()
    {
        var leaderboard = await _context.Users
            .Where(u => u.CompletedRooms > 0)
            .OrderByDescending(u => u.Points)
            .ThenByDescending(u => u.CompletedRooms)
            .Select(u => new
            {
                u.UserId,
                u.Name,
                u.ProfilePhoto,
                u.Points,
                u.CompletedRooms,
                u.Streak,
                Badges = _context.UserBadges
                    .Where(ub => ub.UserId == u.UserId)
                    .Include(ub => ub.Badge)
                    .Select(ub => ub.Badge)
                    .ToList()
            })
            .Take(100)
            .ToListAsync();

        return Ok(leaderboard);
    }

    [HttpGet("my-stats")]
    public async Task<IActionResult> GetMyStats()
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
        var user = await _context.Users.FindAsync(userId);
        
        if (user == null) return NotFound();

        // Calculate Rank
        var rank = await _context.Users.CountAsync(u => u.Points > user.Points) + 1;

        // Retroactive Badge Awarding
        var allBadges = await _context.Badges.ToListAsync();
        var userBadgeIds = await _context.UserBadges.Where(ub => ub.UserId == userId).Select(ub => ub.BadgeId).ToListAsync();
        bool anyNewBadge = false;

        foreach (var badge in allBadges)
        {
            if (user.Points >= badge.PointsRequired && !userBadgeIds.Contains(badge.BadgeId))
            {
                _context.UserBadges.Add(new UserBadge
                {
                    UserId = userId,
                    BadgeId = badge.BadgeId,
                    DateEarned = DateTime.UtcNow
                });
                anyNewBadge = true;
            }
        }

        if (anyNewBadge)
        {
            await _context.SaveChangesAsync();
        }

        var stats = new
        {
            user.Points,
            user.CompletedRooms,
            user.Streak,
            Rank = rank,
            Badges = await _context.UserBadges
                .Where(ub => ub.UserId == userId)
                .Include(ub => ub.Badge)
                .Select(ub => ub.Badge)
                .ToListAsync()
        };

        return Ok(stats);
    }
}
