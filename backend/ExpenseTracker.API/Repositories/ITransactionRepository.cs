using ExpenseTracker.API.Models;

namespace ExpenseTracker.API.Repositories;

/// <summary>
/// Data access interface for Transaction entities.
/// Contains the most complex queries in the system — filtered lists and aggregations.
/// </summary>
public interface ITransactionRepository
{
    Task<Transaction?> GetByIdAsync(int id, int userId);
    Task<(List<Transaction> Items, int Total)> GetFilteredAsync(
        int userId, DateTime? startDate, DateTime? endDate,
        int? categoryId, decimal? minAmount, decimal? maxAmount,
        string? search, int page, int pageSize, string sortBy, string sortDir);
    Task<bool> HashExistsAsync(string hash);
    Task<List<Transaction>> GetByMonthAsync(int userId, int year, int month);
    Task<List<Transaction>> GetByYearAsync(int userId, int year);
    Task<List<Transaction>> GetRecentAsync(int userId, int count);
    Task<List<Transaction>> GetByDateRangeAsync(int userId, DateTime start, DateTime end);
    Task<Transaction> CreateAsync(Transaction transaction);
    Task AddRangeAsync(IEnumerable<Transaction> transactions);
    Task UpdateCategoryAsync(int id, int userId, int categoryId);
    Task DeleteAsync(int id, int userId);

    /// <summary>Used by anomaly detection — returns spending history for comparison.</summary>
    Task<List<Transaction>> GetMerchantHistoryAsync(int userId, string merchant, int limitMonths);
    Task<decimal> GetAverageMonthlySpendingAsync(int userId, int categoryId, int lookbackMonths);

    /// <summary>Returns the date of the most recent transaction for a user, or null if none exist.</summary>
    Task<DateTime?> GetMostRecentDateAsync(int userId);
}
