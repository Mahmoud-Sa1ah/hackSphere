using Microsoft.EntityFrameworkCore;
using PentestHub.API.Data;
using PentestHub.API.Data.Models;

namespace PentestHub.API.Services;

public class DomainService : IDomainService
{
    private readonly ApplicationDbContext _context;
    private readonly IWebHostEnvironment _environment;

    public DomainService(ApplicationDbContext context, IWebHostEnvironment environment)
    {
        _context = context;
        _environment = environment;
    }

    public async Task<DomainVerification> UploadProofAsync(int userId, string domain, IFormFile proofFile)
    {
        // validate domain format roughly
        if (string.IsNullOrWhiteSpace(domain))
             throw new ArgumentException("Domain name is required.");

        // Create directory if not exists
        var uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads", "proofs");
        if (!Directory.Exists(uploadsFolder))
            Directory.CreateDirectory(uploadsFolder);

        // Generate safe filename
        var fileName = $"{Guid.NewGuid()}_{Path.GetFileName(proofFile.FileName)}";
        var filePath = Path.Combine(uploadsFolder, fileName);

        // Save file
        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await proofFile.CopyToAsync(stream);
        }

        var verification = new DomainVerification
        {
            UserId = userId,
            DomainName = domain.ToLower().Trim(),
            ProofPath = $"/uploads/proofs/{fileName}",
            Status = VerificationStatus.Pending,
            CreatedAt = DateTime.UtcNow
        };

        _context.DomainVerifications.Add(verification);
        await _context.SaveChangesAsync();

        return verification;
    }

    public async Task<IEnumerable<DomainVerification>> GetUserVerificationsAsync(int userId)
    {
        return await _context.DomainVerifications
            .Where(v => v.UserId == userId)
            .OrderByDescending(v => v.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<DomainVerification>> GetPendingVerificationsAsync()
    {
        return await _context.DomainVerifications
            .Include(v => v.User)
            .Where(v => v.Status == VerificationStatus.Pending)
            .OrderBy(v => v.CreatedAt)
            .ToListAsync();
    }

    public async Task<DomainVerification?> GetVerificationByIdAsync(int id)
    {
         return await _context.DomainVerifications
            .Include(v => v.User)
            .FirstOrDefaultAsync(v => v.Id == id);
    }

    public async Task<bool> VerifyDomainAsync(int id, bool approve, string? comments = null)
    {
        var verification = await _context.DomainVerifications.FindAsync(id);
        if (verification == null) return false;

        verification.Status = approve ? VerificationStatus.Verified : VerificationStatus.Rejected;
        verification.VerifiedAt = DateTime.UtcNow;
        verification.AdminComments = comments;

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> IsDomainVerifiedAsync(int userId, string domain)
    {
        if (string.IsNullOrWhiteSpace(domain)) return false;

        return await _context.DomainVerifications
            .AnyAsync(v => v.UserId == userId 
                           && v.DomainName == domain.ToLower().Trim() 
                           && v.Status == VerificationStatus.Verified);
    }
}
