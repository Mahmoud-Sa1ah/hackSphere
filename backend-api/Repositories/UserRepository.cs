using Microsoft.EntityFrameworkCore;
using PentestHub.API.Data;
using PentestHub.API.Data.Models;

namespace PentestHub.API.Repositories;

public class UserRepository : IUserRepository
{
    private readonly ApplicationDbContext _context;

    public UserRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<User?> GetByIdAsync(int id)
    {
        return await _context.Users
            .Include(u => u.Role)
            .FirstOrDefaultAsync(u => u.UserId == id);
    }

    public async Task<User?> GetByEmailAsync(string email)
    {
        return await _context.Users
            .Include(u => u.Role)
            .FirstOrDefaultAsync(u => u.Email == email);
    }

    public async Task<User?> GetByUsernameAsync(string username)
    {
        return await _context.Users
            .Include(u => u.Role)
            .FirstOrDefaultAsync(u => u.Name == username);
    }

    public async Task<User?> GetByEmailOrUsernameAsync(string emailOrUsername)
    {
        return await _context.Users
            .Include(u => u.Role)
            .FirstOrDefaultAsync(u => u.Email == emailOrUsername || u.Name == emailOrUsername);
    }

    public async Task<IEnumerable<User>> GetAllAsync()
    {
        return await _context.Users
            .Include(u => u.Role)
            .ToListAsync();
    }

    public async Task AddAsync(User user)
    {
        await _context.Users.AddAsync(user);
    }

    public async Task UpdateAsync(User user)
    {
        _context.Users.Update(user);
        await Task.CompletedTask;
    }

    public async Task DeleteAsync(User user)
    {
        _context.Users.Remove(user);
        await Task.CompletedTask;
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}
