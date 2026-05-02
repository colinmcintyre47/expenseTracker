using ExpenseTracker.API.Data;
using ExpenseTracker.API.Models;
using Microsoft.EntityFrameworkCore;

namespace ExpenseTracker.API.Repositories;

public class BudgetRepository : IBudgetRepository
{
    private readonly AppDbContext _db;
    public BudgetRepository(AppDbContext db) => _db = db;

    public async Task<List<Budget>> GetForUserMonthAsync(int userId, int year, int month)
        => await _db.Budgets
            .Include(b => b.Category)
            .Where(b => b.UserId == userId && b.Year == year && b.Month == month)
            .ToListAsync();

    public async Task<Budget?> GetByIdAsync(int id, int userId)
        => await _db.Budgets.Include(b => b.Category)
            .FirstOrDefaultAsync(b => b.Id == id && b.UserId == userId);

    public async Task<Budget?> GetByCategoryMonthAsync(int userId, int categoryId, int year, int month)
        => await _db.Budgets.FirstOrDefaultAsync(b =>
            b.UserId == userId && b.CategoryId == categoryId &&
            b.Year == year && b.Month == month);

    public async Task<Budget> CreateAsync(Budget budget)
    {
        _db.Budgets.Add(budget);
        await _db.SaveChangesAsync();
        return budget;
    }

    public async Task UpdateAsync(Budget budget)
    {
        _db.Budgets.Update(budget);
        await _db.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id, int userId)
    {
        var budget = await _db.Budgets.FirstOrDefaultAsync(b => b.Id == id && b.UserId == userId);
        if (budget == null) return;
        _db.Budgets.Remove(budget);
        await _db.SaveChangesAsync();
    }
}
