using ExpenseTracker.API.Models;

namespace ExpenseTracker.API.Repositories;

/// <summary>
/// Data access interface for User entities.
/// Using an interface here allows unit tests to mock the database layer.
/// </summary>
public interface IUserRepository
{
    Task<User?> GetByIdAsync(int id);
    Task<User?> GetByEmailAsync(string email);
    Task<bool> EmailExistsAsync(string email);
    Task<User> CreateAsync(User user);
}
