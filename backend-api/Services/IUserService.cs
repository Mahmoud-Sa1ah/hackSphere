using PentestHub.API.Data.DTOs;

namespace PentestHub.API.Services;

public interface IUserService
{
    Task<UserDTO?> GetUserByIdAsync(int userId);
    Task<bool> UpdateProfilePhotoAsync(int userId, string photoUrl);
    Task<bool> DeleteProfilePhotoAsync(int userId);
    Task<bool> UpdateBioAsync(int userId, string bio);
    Task<bool> UpdateNameAsync(int userId, string name);
    Task<bool> DeleteAccountAsync(int userId);
}
