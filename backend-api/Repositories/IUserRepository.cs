using PentestHub.API.Data.Models;

namespace PentestHub.API.Repositories;

public interface IUserRepository
{
    Task<User?> GetByIdAsync(int id);
    Task<User?> GetByEmailAsync(string email);
    Task<User?> GetByUsernameAsync(string username);
    Task<User?> GetByEmailOrUsernameAsync(string emailOrUsername);
    Task<IEnumerable<User>> GetAllAsync();
    Task AddAsync(User user);
    Task UpdateAsync(User user);
    Task DeleteAsync(User user);
    Task SaveChangesAsync();
}
