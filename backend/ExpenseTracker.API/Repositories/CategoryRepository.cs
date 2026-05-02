using ExpenseTracker.API.Data;
using ExpenseTracker.API.Models;
using Microsoft.EntityFrameworkCore;

namespace ExpenseTracker.API.Repositories;

public class CategoryRepository : ICategoryRepository
{
    private readonly AppDbContext _db;
    public CategoryRepository(AppDbContext db) => _db = db;

    /// <summary>
    /// Returns categories available to a user: system defaults (UserId == null)
    /// plus any custom categories they created (UserId == userId).
    /// </summary>
    public async Task<List<Category>> GetForUserAsync(int userId)
        => await _db.Categories
            .Where(c => c.UserId == null || c.UserId == userId)
            .OrderBy(c => c.IsSystem ? 0 : 1) // System categories first
            .ThenBy(c => c.Name)
            .ToListAsync();

    public async Task<Category?> GetByIdAsync(int id)
        => await _db.Categories.FindAsync(id);

    public async Task<Category> CreateAsync(Category category)
    {
        _db.Categories.Add(category);
        await _db.SaveChangesAsync();
        return category;
    }

    public async Task UpdateAsync(Category category)
    {
        _db.Categories.Update(category);
        await _db.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id, int userId)
    {
        var category = await _db.Categories
            .FirstOrDefaultAsync(c => c.Id == id && c.UserId == userId && !c.IsSystem);
        if (category == null) return;
        _db.Categories.Remove(category);
        await _db.SaveChangesAsync();
    }

    /// <summary>Returns the "Uncategorized" system category used as default fallback.</summary>
    public async Task<Category?> GetUncategorizedAsync()
        => await _db.Categories.FirstOrDefaultAsync(c => c.IsSystem && c.Name == "Uncategorized");
}
