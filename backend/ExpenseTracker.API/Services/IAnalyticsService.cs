using ExpenseTracker.API.DTOs.Analytics;

namespace ExpenseTracker.API.Services;

public interface IAnalyticsService
{
    Task<DashboardSummaryDto> GetDashboardSummaryAsync(int userId);
    Task<MonthlyAnalyticsDto> GetMonthlyAnalyticsAsync(int userId, int year, int month);
    Task<YearlyAnalyticsDto> GetYearlyAnalyticsAsync(int userId, int year);
    Task<TrendsDto> GetTrendsAsync(int userId);
}
