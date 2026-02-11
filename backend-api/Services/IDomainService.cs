using PentestHub.API.Data.Models;

namespace PentestHub.API.Services;

public interface IDomainService
{
    Task<DomainVerification> UploadProofAsync(int userId, string domain, IFormFile proofFile);
    Task<IEnumerable<DomainVerification>> GetUserVerificationsAsync(int userId);
    Task<IEnumerable<DomainVerification>> GetPendingVerificationsAsync();
    Task<DomainVerification?> GetVerificationByIdAsync(int id);
    Task<bool> VerifyDomainAsync(int id, bool approve, string? comments = null);
    Task<bool> IsDomainVerifiedAsync(int userId, string domain);
}
