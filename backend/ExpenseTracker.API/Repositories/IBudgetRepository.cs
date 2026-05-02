using ExpenseTracker.API.Models;

namespace ExpenseTracker.API.Repositories;

public interface IBudgetRepository
{
    Task<List<Budget>> GetForUserMonthAsync(int userId, int year, int month);
    Task<Budget?> GetByIdAsync(int id, int userId);
    Task<Budget?> GetByCategoryMonthAsync(int userId, int categoryId, int year, int month);
    Task<Budget> CreateAsync(Budget budget);
    Task UpdateAsync(Budget budget);
    Task DeleteAsync(int id, int userId);
}
