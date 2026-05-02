using ExpenseTracker.API.Data;
using ExpenseTracker.API.Models;
using Microsoft.EntityFrameworkCore;

namespace ExpenseTracker.API.Repositories;

/// <summary>
/// EF Core implementation of ITransactionRepository.
/// All queries include .Include(t => t.Category) because the frontend always
/// needs category name/color alongside transaction data.
/// </summary>
public class TransactionRepository : ITransactionRepository
{
    private readonly AppDbContext _db;

    public TransactionRepository(AppDbContext db)
    {
        _db = db;
    }

    public async Task<Transaction?> GetByIdAsync(int id, int userId)
        => await _db.Transactions
            .Include(t => t.Category)
            .FirstOrDefaultAsync(t => t.Id == id && t.UserId == userId);

    /// <summary>
    /// Builds a dynamic query based on the filters provided.
    /// All filters are optional — omitting them returns all transactions for the user.
    /// The query is paged to avoid loading thousands of rows into memory.
    /// </summary>
    public async Task<(List<Transaction> Items, int Total)> GetFilteredAsync(
        int userId, DateTime? startDate, DateTime? endDate,
        int? categoryId, decimal? minAmount, decimal? maxAmount,
        string? search, int page, int pageSize, string sortBy, string sortDir)
    {
        // Start with all transactions for this user
        var query = _db.Transactions
            .Include(t => t.Category)
            .Where(t => t.UserId == userId)
            .AsQueryable();

        // Apply optional filters
        if (startDate.HasValue)
            query = query.Where(t => t.Date >= startDate.Value);

        if (endDate.HasValue)
            query = query.Where(t => t.Date <= endDate.Value);

        if (categoryId.HasValue)
            query = query.Where(t => t.CategoryId == categoryId.Value);

        if (minAmount.HasValue)
            query = query.Where(t => t.Amount >= minAmount.Value);

        if (maxAmount.HasValue)
            query = query.Where(t => t.Amount <= maxAmount.Value);

        if (!string.IsNullOrWhiteSpace(search))
        {
            // Case-insensitive search against merchant and description
            var term = search.Trim().ToLower();
            query = query.Where(t =>
                t.Merchant.ToLower().Contains(term) ||
                t.Description.ToLower().Contains(term));
        }

        // Count total BEFORE paging (needed for pagination UI)
        var total = await query.CountAsync();

        // Apply sorting
        query = (sortBy.ToLower(), sortDir.ToLower()) switch
        {
            ("amount", "asc")  => query.OrderBy(t => t.Amount),
            ("amount", "desc") => query.OrderByDescending(t => t.Amount),
            ("merchant", "asc")  => query.OrderBy(t => t.Merchant),
            ("merchant", "desc") => query.OrderByDescending(t => t.Merchant),
            ("date", "asc")    => query.OrderBy(t => t.Date),
            _                  => query.OrderByDescending(t => t.Date) // default: newest first
        };

        // Apply pagination — Skip moves past previous pages, Take limits the result set
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, total);
    }

    public async Task<bool> HashExistsAsync(string hash)
        => await _db.Transactions.AnyAsync(t => t.ImportHash == hash);

    public async Task<List<Transaction>> GetByMonthAsync(int userId, int year, int month)
        => await _db.Transactions
            .Include(t => t.Category)
            .Where(t => t.UserId == userId && t.Date.Year == year && t.Date.Month == month)
            .OrderByDescending(t => t.Date)
            .ToListAsync();

    public async Task<List<Transaction>> GetByYearAsync(int userId, int year)
        => await _db.Transactions
            .Include(t => t.Category)
            .Where(t => t.UserId == userId && t.Date.Year == year)
            .OrderBy(t => t.Date)
            .ToListAsync();

    public async Task<List<Transaction>> GetRecentAsync(int userId, int count)
        => await _db.Transactions
            .Include(t => t.Category)
            .Where(t => t.UserId == userId)
            .OrderByDescending(t => t.Date)
            .Take(count)
            .ToListAsync();

    public async Task<List<Transaction>> GetByDateRangeAsync(int userId, DateTime start, DateTime end)
        => await _db.Transactions
            .Include(t => t.Category)
            .Where(t => t.UserId == userId && t.Date >= start && t.Date <= end)
            .OrderBy(t => t.Date)
            .ToListAsync();

    public async Task<Transaction> CreateAsync(Transaction transaction)
    {
        _db.Transactions.Add(transaction);
        await _db.SaveChangesAsync();
        return transaction;
    }

    public async Task AddRangeAsync(IEnumerable<Transaction> transactions)
    {
        await _db.Transactions.AddRangeAsync(transactions);
        await _db.SaveChangesAsync();
    }

    public async Task UpdateCategoryAsync(int id, int userId, int categoryId)
    {
        var transaction = await _db.Transactions
            .FirstOrDefaultAsync(t => t.Id == id && t.UserId == userId);
        if (transaction == null) return;
        transaction.CategoryId = categoryId;
        await _db.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id, int userId)
    {
        var transaction = await _db.Transactions
            .FirstOrDefaultAsync(t => t.Id == id && t.UserId == userId);
        if (transaction == null) return;
        _db.Transactions.Remove(transaction);
        await _db.SaveChangesAsync();
    }

    public async Task<List<Transaction>> GetMerchantHistoryAsync(int userId, string merchant, int limitMonths)
    {
        var cutoff = DateTime.UtcNow.AddMonths(-limitMonths);
        return await _db.Transactions
            .Where(t => t.UserId == userId &&
                        t.Merchant.ToLower() == merchant.ToLower() &&
                        t.Date >= cutoff)
            .OrderByDescending(t => t.Date)
            .ToListAsync();
    }

    public async Task<DateTime?> GetMostRecentDateAsync(int userId)
        => await _db.Transactions
            .Where(t => t.UserId == userId)
            .OrderByDescending(t => t.Date)
            .Select(t => (DateTime?)t.Date)
            .FirstOrDefaultAsync();

    public async Task<decimal> GetAverageMonthlySpendingAsync(int userId, int categoryId, int lookbackMonths)
    {
        var cutoff = DateTime.UtcNow.AddMonths(-lookbackMonths);
        var transactions = await _db.Transactions
            .Where(t => t.UserId == userId &&
                        t.CategoryId == categoryId &&
                        t.Date >= cutoff &&
                        t.TransactionType == "Debit")
            .ToListAsync();

        if (!transactions.Any()) return 0;

        // Group by month, sum each month, then average
        var monthlyTotals = transactions
            .GroupBy(t => new { t.Date.Year, t.Date.Month })
            .Select(g => g.Sum(t => t.Amount));

        return monthlyTotals.Average();
    }
}
