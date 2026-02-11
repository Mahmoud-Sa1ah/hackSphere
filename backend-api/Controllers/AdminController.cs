using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PentestHub.API.Data;
using PentestHub.API.Data.Models;
using PentestHub.API.Services;
using System.Security.Claims;

namespace PentestHub.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class AdminController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly INotificationService _notificationService;
    private readonly IDomainService _domainService;

    public AdminController(ApplicationDbContext context, INotificationService notificationService, IDomainService domainService)
    {
        _context = context;
        _notificationService = notificationService;
        _domainService = domainService;
    }

    [HttpGet("users")]
    public async Task<IActionResult> GetAllUsers()
    {
        var users = await _context.Users
            .Include(u => u.Role)
            .Select(u => new
            {
                u.UserId,
                u.Name,
                u.Email,
                Role = u.Role != null ? u.Role.RoleName : "Unknown",
                u.CreatedAt
            })
            .ToListAsync();

        return Ok(users);
    }

    [HttpDelete("users/{id}")]
    public async Task<IActionResult> DeleteUser(int id)
    {
        var currentUserId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
        
        // Prevent self-deletion
        if (id == currentUserId)
        {
            return BadRequest(new { message = "You cannot delete your own account" });
        }

        var user = await _context.Users.FindAsync(id);
        if (user == null)
        {
            return NotFound();
        }

        // Delete related data
        var scans = await _context.ScanHistory.Where(s => s.UserId == id).ToListAsync();
        _context.ScanHistory.RemoveRange(scans);

        var notifications = await _context.Notifications.Where(n => n.UserId == id).ToListAsync();
        _context.Notifications.RemoveRange(notifications);

        var labResults = await _context.LabResults.Where(lr => lr.UserId == id).ToListAsync();
        _context.LabResults.RemoveRange(labResults);

        var aiConversations = await _context.AIConversations.Where(ac => ac.UserId == id).ToListAsync();
        _context.AIConversations.RemoveRange(aiConversations);

        var domainVerifications = await _context.DomainVerifications.Where(dv => dv.UserId == id).ToListAsync();
        _context.DomainVerifications.RemoveRange(domainVerifications);

        var userBadges = await _context.UserBadges.Where(ub => ub.UserId == id).ToListAsync();
        _context.UserBadges.RemoveRange(userBadges);

        var reports = await _context.Reports.Where(r => r.UserId == id).ToListAsync();
        _context.Reports.RemoveRange(reports);

        _context.Users.Remove(user);
        await _context.SaveChangesAsync();

        return Ok(new { message = "User deleted successfully" });
    }

    [HttpPost("notifications")]
    public async Task<IActionResult> SendNotification([FromBody] SendNotificationDto dto)
    {
        if (dto == null)
        {
            return BadRequest(new { message = "Request body is required" });
        }

        if (dto.UserId <= 0)
        {
            return BadRequest(new { message = "Valid UserId is required" });
        }

        if (string.IsNullOrWhiteSpace(dto.Message))
        {
            return BadRequest(new { message = "Message is required" });
        }

        // Verify user exists
        var userExists = await _context.Users.AnyAsync(u => u.UserId == dto.UserId);
        if (!userExists)
        {
            return NotFound(new { message = "User not found" });
        }

        try
        {
            await _notificationService.CreateNotificationAsync(
                dto.UserId,
                dto.Title ?? "Admin Notification",
                dto.Message
            );

            return Ok(new { message = "Notification sent successfully" });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = $"Failed to send notification: {ex.Message}" });
        }
    }

    // Domain Verification API
    [HttpGet("domains/pending")]
    public async Task<IActionResult> GetPendingDomains()
    {
        var domains = await _domainService.GetPendingVerificationsAsync();
        return Ok(domains);
    }

    [HttpPost("domains/{id}/verify")]
    public async Task<IActionResult> VerifyDomain(int id, [FromBody] VerifyDomainDto dto)
    {
        var result = await _domainService.VerifyDomainAsync(id, dto.Approve, dto.Comments);
        if (!result) return NotFound(new { message = "Verification request not found" });

        return Ok(new { message = dto.Approve ? "Domain approved" : "Domain rejected" });
    }

    // Knowledge Base Management
    [HttpGet("knowledge")]
    public async Task<IActionResult> GetAllKnowledgeArticles()
    {
        var articles = await _context.KnowledgeBases
            .Where(k => k.IsPublished)
            .OrderByDescending(k => k.CreatedAt)
            .Select(k => new
            {
                k.KnowledgeBaseId,
                k.Title,
                k.Category,
                k.Content,
                k.Tags,
                k.IsPublished,
                k.CreatedAt,
                k.UpdatedAt
            })
            .ToListAsync();

        return Ok(articles);
    }

    [HttpGet("knowledge/all")]
    public async Task<IActionResult> GetAllKnowledgeArticlesAdmin()
    {
        var articles = await _context.KnowledgeBases
            .OrderByDescending(k => k.CreatedAt)
            .Select(k => new
            {
                k.KnowledgeBaseId,
                k.Title,
                k.Category,
                k.Content,
                k.Tags,
                k.IsPublished,
                k.CreatedAt,
                k.UpdatedAt,
                k.CreatedBy
            })
            .ToListAsync();

        return Ok(articles);
    }

    [HttpPost("knowledge")]
    public async Task<IActionResult> CreateKnowledgeArticle([FromBody] CreateKnowledgeDto dto)
    {
        try
        {
            if (dto == null || string.IsNullOrWhiteSpace(dto.Title) || string.IsNullOrWhiteSpace(dto.Content))
            {
                return BadRequest(new { message = "Title and Content are required" });
            }

            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId) || userId == 0)
            {
                return Unauthorized(new { message = "Invalid user authentication" });
            }

            // Ensure KnowledgeBase table exists
            try
            {
                await _context.Database.ExecuteSqlRawAsync("SELECT 1 FROM KnowledgeBases LIMIT 1");
            }
            catch
            {
                // Table doesn't exist, create it manually
                await _context.Database.ExecuteSqlRawAsync(@"
                    CREATE TABLE IF NOT EXISTS KnowledgeBases (
                        KnowledgeBaseId INT AUTO_INCREMENT PRIMARY KEY,
                        Title VARCHAR(200) NOT NULL,
                        Category VARCHAR(100) NOT NULL,
                        Content TEXT NOT NULL,
                        Tags VARCHAR(500),
                        IsPublished BOOLEAN NOT NULL DEFAULT TRUE,
                        CreatedAt DATETIME(6) NOT NULL,
                        UpdatedAt DATETIME(6) NULL,
                        CreatedBy INT NULL
                    ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;
                ");
            }

            var article = new KnowledgeBase
            {
                Title = dto.Title,
                Category = dto.Category ?? "General",
                Content = dto.Content,
                Tags = dto.Tags ?? "",
                IsPublished = dto.IsPublished ?? true,
                CreatedBy = userId,
                CreatedAt = DateTime.UtcNow
            };

            _context.KnowledgeBases.Add(article);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Knowledge article created successfully", articleId = article.KnowledgeBaseId });
        }
        catch (DbUpdateException dbEx)
        {
            var innerException = dbEx.InnerException?.Message ?? dbEx.Message;
            return StatusCode(500, new { message = $"Database error: {innerException}" });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = $"Failed to create knowledge article: {ex.Message}", details = ex.ToString() });
        }
    }

    [HttpPut("knowledge/{id}")]
    public async Task<IActionResult> UpdateKnowledgeArticle(int id, [FromBody] UpdateKnowledgeDto dto)
    {
        var article = await _context.KnowledgeBases.FindAsync(id);
        if (article == null)
        {
            return NotFound(new { message = "Knowledge article not found" });
        }

        if (!string.IsNullOrWhiteSpace(dto.Title))
            article.Title = dto.Title;
        if (!string.IsNullOrWhiteSpace(dto.Category))
            article.Category = dto.Category;
        if (!string.IsNullOrWhiteSpace(dto.Content))
            article.Content = dto.Content;
        if (dto.Tags != null)
            article.Tags = dto.Tags;
        if (dto.IsPublished.HasValue)
            article.IsPublished = dto.IsPublished.Value;
        
        article.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return Ok(new { message = "Knowledge article updated successfully" });
    }

    [HttpDelete("knowledge/{id}")]
    public async Task<IActionResult> DeleteKnowledgeArticle(int id)
    {
        var article = await _context.KnowledgeBases.FindAsync(id);
        if (article == null)
        {
            return NotFound(new { message = "Knowledge article not found" });
        }

        _context.KnowledgeBases.Remove(article);
        await _context.SaveChangesAsync();

        return Ok(new { message = "Knowledge article deleted successfully" });
    }

    [HttpPost("knowledge/upload-image")]
    public async Task<IActionResult> UploadKnowledgeImage(IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            return BadRequest(new { message = "No file uploaded" });
        }

        // Validate file type
        var allowedTypes = new[] { "image/jpeg", "image/png", "image/gif", "image/webp" };
        if (!allowedTypes.Contains(file.ContentType.ToLower()))
        {
            return BadRequest(new { message = "Invalid file type. Only JPEG, PNG, GIF, and WebP are allowed." });
        }

        // Validate file size (max 5MB)
        if (file.Length > 5 * 1024 * 1024)
        {
            return BadRequest(new { message = "File size must be less than 5MB" });
        }

        try
        {
            // Create uploads directory if it doesn't exist
            var uploadsPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "knowledge");
            if (!Directory.Exists(uploadsPath))
            {
                Directory.CreateDirectory(uploadsPath);
            }

            // Generate unique filename
            var fileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
            var filePath = Path.Combine(uploadsPath, fileName);

            // Save the file
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            // Return the URL to the uploaded image
            var imageUrl = $"/uploads/knowledge/{fileName}";
            return Ok(new { url = imageUrl, fileName = fileName });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = $"Failed to upload image: {ex.Message}" });
        }
    }
}

public class SendNotificationDto
{
    public int UserId { get; set; }
    public string? Title { get; set; }
    public string Message { get; set; } = string.Empty;
}

public class CreateKnowledgeDto
{
    public string Title { get; set; } = string.Empty;
    public string? Category { get; set; }
    public string Content { get; set; } = string.Empty;
    public string? Tags { get; set; }
    public bool? IsPublished { get; set; }
}

public class UpdateKnowledgeDto
{
    public string? Title { get; set; }
    public string? Category { get; set; }
    public string? Content { get; set; }
    public string? Tags { get; set; }
    public bool? IsPublished { get; set; }
}

public class VerifyDomainDto
{
    public bool Approve { get; set; }
    public string? Comments { get; set; }
}



