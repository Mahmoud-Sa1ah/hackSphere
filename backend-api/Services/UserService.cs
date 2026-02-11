using PentestHub.API.Data.DTOs;
using PentestHub.API.Repositories;
using PentestHub.API.Data.Models;

namespace PentestHub.API.Services;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;

    public UserService(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<UserDTO?> GetUserByIdAsync(int userId)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null) return null;

        return new UserDTO
        {
            UserId = user.UserId,
            Name = user.Name,
            Email = user.Email,
            Role = user.Role?.RoleName ?? "Learner",
            ProfilePhoto = user.ProfilePhoto,
            Bio = user.Bio,
            Points = user.Points,
            CompletedRooms = user.CompletedRooms,
            Streak = user.Streak
        };
    }

    public async Task<bool> UpdateProfilePhotoAsync(int userId, string photoUrl)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null) return false;

        user.ProfilePhoto = photoUrl;
        await _userRepository.UpdateAsync(user);
        await _userRepository.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteProfilePhotoAsync(int userId)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null) return false;

        user.ProfilePhoto = null;
        await _userRepository.UpdateAsync(user);
        await _userRepository.SaveChangesAsync();
        return true;
    }

    public async Task<bool> UpdateBioAsync(int userId, string bio)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null) return false;

        user.Bio = bio;
        await _userRepository.UpdateAsync(user);
        await _userRepository.SaveChangesAsync();
        return true;
    }

    public async Task<bool> UpdateNameAsync(int userId, string name)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null) return false;

        user.Name = name;
        await _userRepository.UpdateAsync(user);
        await _userRepository.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteAccountAsync(int userId)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null) return false;

        await _userRepository.DeleteAsync(user);
        await _userRepository.SaveChangesAsync();
        return true;
    }
}
