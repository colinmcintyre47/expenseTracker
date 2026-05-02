using ExpenseTracker.API.Models;

namespace ExpenseTracker.API.Repositories;

public interface ICategoryRepository
{
    /// <summary>Returns system categories + categories owned by the user.</summary>
    Task<List<Category>> GetForUserAsync(int userId);
    Task<Category?> GetByIdAsync(int id);
    Task<Category> CreateAsync(Category category);
    Task UpdateAsync(Category category);
    Task DeleteAsync(int id, int userId);
    Task<Category?> GetUncategorizedAsync();
}
