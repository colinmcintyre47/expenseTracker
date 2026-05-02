namespace ExpenseTracker.API.DTOs.Analytics;

/// <summary>Response for GET /api/analytics/monthly?year=2025&amp;month=3</summary>
public class MonthlyAnalyticsDto
{
    public int Year { get; set; }
    public int Month { get; set; }
    public string MonthName { get; set; } = string.Empty;
    public decimal TotalSpent { get; set; }
    public decimal TotalIncome { get; set; }
    public decimal NetAmount { get; set; }
    public int TransactionCount { get; set; }
    public decimal AverageTransactionAmount { get; set; }

    /// <summary>Spending broken down by category for the pie chart.</summary>
    public List<CategorySpendingDto> CategoryBreakdown { get; set; } = new();

    /// <summary>Largest single transactions for the "Big Purchases" section.</summary>
    public List<TopTransactionDto> TopTransactions { get; set; } = new();
}

/// <summary>Response for GET /api/analytics/yearly?year=2025</summary>
public class YearlyAnalyticsDto
{
    public int Year { get; set; }
    public decimal TotalSpent { get; set; }
    public decimal TotalIncome { get; set; }
    public List<MonthSummaryDto> MonthlyBreakdown { get; set; } = new();
    public List<CategorySpendingDto> CategoryBreakdown { get; set; } = new();
}

/// <summary>Single month summary used in the yearly breakdown.</summary>
public class MonthSummaryDto
{
    public int Month { get; set; }
    public string MonthName { get; set; } = string.Empty;
    public decimal TotalSpent { get; set; }
    public decimal TotalIncome { get; set; }
}

/// <summary>Category spending item for pie/bar charts.</summary>
public class CategorySpendingDto
{
    public int CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public string CategoryColor { get; set; } = string.Empty;
    public string CategoryIcon { get; set; } = string.Empty;
    public decimal TotalSpent { get; set; }
    public decimal Percentage { get; set; }
}

/// <summary>Top transaction summary for dashboard widget.</summary>
public class TopTransactionDto
{
    public int Id { get; set; }
    public string Merchant { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public DateTime Date { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public string CategoryColor { get; set; } = string.Empty;
}

/// <summary>Response for GET /api/analytics/trends — 6-month rolling view.</summary>
public class TrendsDto
{
    public List<MonthTrendDto> Months { get; set; } = new();
}

public class MonthTrendDto
{
    public int Year { get; set; }
    public int Month { get; set; }
    public string MonthName { get; set; } = string.Empty;
    public decimal TotalSpent { get; set; }
    public decimal TotalIncome { get; set; }
}

/// <summary>Dashboard summary returned by GET /api/analytics/dashboard</summary>
public class DashboardSummaryDto
{
    public decimal TotalSpentThisMonth { get; set; }
    public decimal TotalSpentLastMonth { get; set; }
    public decimal MonthOverMonthChange { get; set; } // percentage
    public int TransactionCountThisMonth { get; set; }
    public int UnreadAlertCount { get; set; }
    public List<CategorySpendingDto> SpendingByCategory { get; set; } = new();
    public List<TopTransactionDto> LargestTransactions { get; set; } = new();
    public List<TopTransactionDto> RecentTransactions { get; set; } = new();
    public List<MonthTrendDto> SpendingTrend { get; set; } = new();
}
