using ExpenseTracker.API.Data;
using ExpenseTracker.API.Models;
using Microsoft.EntityFrameworkCore;

namespace ExpenseTracker.API.Repositories;

/// <summary>
/// EF Core implementation of IUserRepository.
/// All database queries go through AppDbContext.
/// </summary>
public class UserRepository : IUserRepository
{
    private readonly AppDbContext _db;

    public UserRepository(AppDbContext db)
    {
        _db = db;
    }

    public async Task<User?> GetByIdAsync(int id)
        => await _db.Users.FindAsync(id);

    public async Task<User?> GetByEmailAsync(string email)
        => await _db.Users.FirstOrDefaultAsync(u => u.Email == email.ToLowerInvariant());

    public async Task<bool> EmailExistsAsync(string email)
        => await _db.Users.AnyAsync(u => u.Email == email.ToLowerInvariant());

    public async Task<User> CreateAsync(User user)
    {
        // Normalize email to lowercase for consistent lookup
        user.Email = user.Email.ToLowerInvariant();
        _db.Users.Add(user);
        await _db.SaveChangesAsync();
        return user;
    }
}
