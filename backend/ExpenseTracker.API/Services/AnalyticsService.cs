using ExpenseTracker.API.DTOs.Analytics;
using ExpenseTracker.API.Models;
using ExpenseTracker.API.Repositories;

namespace ExpenseTracker.API.Services;

/// <summary>
/// Computes all analytics and dashboard data.
/// All calculations happen in-memory after loading from the database,
/// which is simple and fast for typical personal finance volumes (< 10K transactions/year).
/// For very large datasets, these could be moved to SQL aggregation queries.
/// </summary>
public class AnalyticsService : IAnalyticsService
{
    private readonly ITransactionRepository _transactionRepo;
    private readonly IAlertRepository _alertRepo;

    public AnalyticsService(ITransactionRepository transactionRepo, IAlertRepository alertRepo)
    {
        _transactionRepo = transactionRepo;
        _alertRepo = alertRepo;
    }

    public async Task<DashboardSummaryDto> GetDashboardSummaryAsync(int userId)
    {
        // Use current month, but fall back to the most recent month with data
        // so the dashboard is never blank after a historical import.
        var now = DateTime.UtcNow;
        var mostRecent = await _transactionRepo.GetMostRecentDateAsync(userId);
        var referenceDate = (mostRecent.HasValue && mostRecent.Value < now)
            ? mostRecent.Value
            : now;

        // This month's and last month's transactions
        var thisMonth = await _transactionRepo.GetByMonthAsync(userId, referenceDate.Year, referenceDate.Month);
        var lastMonthDate = referenceDate.AddMonths(-1);
        var lastMonth = await _transactionRepo.GetByMonthAsync(userId, lastMonthDate.Year, lastMonthDate.Month);
        var unreadAlerts = await _alertRepo.GetUnreadCountAsync(userId);

        var thisMonthSpent = SumDebits(thisMonth);
        var lastMonthSpent = SumDebits(lastMonth);

        // Month-over-month change as a percentage
        var momChange = lastMonthSpent > 0
            ? ((thisMonthSpent - lastMonthSpent) / lastMonthSpent) * 100
            : 0;

        // 6-month trend for the line chart, anchored to the reference date
        var trend = new List<MonthTrendDto>();
        for (int i = 5; i >= 0; i--)
        {
            var monthDate = referenceDate.AddMonths(-i);
            var txns = await _transactionRepo.GetByMonthAsync(userId, monthDate.Year, monthDate.Month);
            trend.Add(new MonthTrendDto
            {
                Year = monthDate.Year,
                Month = monthDate.Month,
                MonthName = monthDate.ToString("MMM yyyy"),
                TotalSpent = SumDebits(txns),
                TotalIncome = SumCredits(txns)
            });
        }

        return new DashboardSummaryDto
        {
            TotalSpentThisMonth = thisMonthSpent,
            TotalSpentLastMonth = lastMonthSpent,
            MonthOverMonthChange = Math.Round(momChange, 1),
            TransactionCountThisMonth = thisMonth.Count(t => t.TransactionType == "Debit"),
            UnreadAlertCount = unreadAlerts,
            SpendingByCategory = BuildCategoryBreakdown(thisMonth),
            LargestTransactions = thisMonth
                .Where(t => t.TransactionType == "Debit")
                .OrderByDescending(t => t.Amount)
                .Take(5)
                .Select(MapToTopTransaction)
                .ToList(),
            RecentTransactions = thisMonth
                .OrderByDescending(t => t.Date)
                .Take(10)
                .Select(MapToTopTransaction)
                .ToList(),
            SpendingTrend = trend
        };
    }

    public async Task<MonthlyAnalyticsDto> GetMonthlyAnalyticsAsync(int userId, int year, int month)
    {
        var transactions = await _transactionRepo.GetByMonthAsync(userId, year, month);
        var debits = transactions.Where(t => t.TransactionType == "Debit").ToList();
        var credits = transactions.Where(t => t.TransactionType == "Credit").ToList();

        var totalSpent = debits.Sum(t => t.Amount);
        var totalIncome = credits.Sum(t => t.Amount);

        return new MonthlyAnalyticsDto
        {
            Year = year,
            Month = month,
            MonthName = new DateTime(year, month, 1).ToString("MMMM yyyy"),
            TotalSpent = totalSpent,
            TotalIncome = totalIncome,
            NetAmount = totalIncome - totalSpent,
            TransactionCount = debits.Count,
            AverageTransactionAmount = debits.Any() ? totalSpent / debits.Count : 0,
            CategoryBreakdown = BuildCategoryBreakdown(transactions),
            TopTransactions = debits
                .OrderByDescending(t => t.Amount)
                .Take(10)
                .Select(MapToTopTransaction)
                .ToList()
        };
    }

    public async Task<YearlyAnalyticsDto> GetYearlyAnalyticsAsync(int userId, int year)
    {
        var transactions = await _transactionRepo.GetByYearAsync(userId, year);

        // Group by month for the bar chart
        var monthlyBreakdown = Enumerable.Range(1, 12).Select(m =>
        {
            var monthTxns = transactions.Where(t => t.Date.Month == m).ToList();
            return new MonthSummaryDto
            {
                Month = m,
                MonthName = new DateTime(year, m, 1).ToString("MMM"),
                TotalSpent = SumDebits(monthTxns),
                TotalIncome = SumCredits(monthTxns)
            };
        }).ToList();

        return new YearlyAnalyticsDto
        {
            Year = year,
            TotalSpent = SumDebits(transactions),
            TotalIncome = SumCredits(transactions),
            MonthlyBreakdown = monthlyBreakdown,
            CategoryBreakdown = BuildCategoryBreakdown(transactions)
        };
    }

    public async Task<TrendsDto> GetTrendsAsync(int userId)
    {
        var now = DateTime.UtcNow;
        var months = new List<MonthTrendDto>();

        for (int i = 5; i >= 0; i--)
        {
            var date = now.AddMonths(-i);
            var txns = await _transactionRepo.GetByMonthAsync(userId, date.Year, date.Month);
            months.Add(new MonthTrendDto
            {
                Year = date.Year,
                Month = date.Month,
                MonthName = date.ToString("MMM yyyy"),
                TotalSpent = SumDebits(txns),
                TotalIncome = SumCredits(txns)
            });
        }

        return new TrendsDto { Months = months };
    }

    // -------------------------------------------------------------------
    // Private helpers
    // -------------------------------------------------------------------

    private static decimal SumDebits(IEnumerable<Transaction> txns)
        => txns.Where(t => t.TransactionType == "Debit").Sum(t => t.Amount);

    private static decimal SumCredits(IEnumerable<Transaction> txns)
        => txns.Where(t => t.TransactionType == "Credit").Sum(t => t.Amount);

    /// <summary>
    /// Groups transactions by category and calculates each category's share
    /// of total spending (for pie chart display).
    /// </summary>
    private static List<CategorySpendingDto> BuildCategoryBreakdown(List<Transaction> transactions)
    {
        var debits = transactions.Where(t => t.TransactionType == "Debit").ToList();
        var total = debits.Sum(t => t.Amount);
        if (total == 0) return new List<CategorySpendingDto>();

        return debits
            .GroupBy(t => t.Category)
            .Where(g => g.Key != null)
            .Select(g => new CategorySpendingDto
            {
                CategoryId = g.Key.Id,
                CategoryName = g.Key.Name,
                CategoryColor = g.Key.Color,
                CategoryIcon = g.Key.Icon,
                TotalSpent = g.Sum(t => t.Amount),
                Percentage = Math.Round(g.Sum(t => t.Amount) / total * 100, 1)
            })
            .OrderByDescending(c => c.TotalSpent)
            .ToList();
    }

    private static TopTransactionDto MapToTopTransaction(Transaction t) => new()
    {
        Id = t.Id,
        Merchant = t.Merchant,
        Amount = t.Amount,
        Date = t.Date,
        CategoryName = t.Category?.Name ?? "Unknown",
        CategoryColor = t.Category?.Color ?? "#6B7280"
    };
}
