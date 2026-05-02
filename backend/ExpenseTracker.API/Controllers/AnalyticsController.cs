using ExpenseTracker.API.Services;
using Microsoft.AspNetCore.Mvc;

namespace ExpenseTracker.API.Controllers;

/// <summary>
/// Analytics endpoints providing dashboard data, monthly/yearly breakdowns, and trends.
/// All data is scoped to the authenticated user.
/// </summary>
[Route("api/analytics")]
public class AnalyticsController : BaseAuthController
{
    private readonly IAnalyticsService _analyticsService;

    public AnalyticsController(IAnalyticsService analyticsService)
    {
        _analyticsService = analyticsService;
    }

    /// <summary>
    /// Dashboard summary — total spending, recent transactions, charts data.
    /// This is the first API call the dashboard page makes.
    /// </summary>
    [HttpGet("dashboard")]
    public async Task<IActionResult> GetDashboard()
    {
        var summary = await _analyticsService.GetDashboardSummaryAsync(UserId);
        return Ok(summary);
    }

    /// <summary>
    /// Monthly analytics breakdown.
    /// Defaults to current month if year/month are not provided.
    /// </summary>
    [HttpGet("monthly")]
    public async Task<IActionResult> GetMonthly([FromQuery] int? year, [FromQuery] int? month)
    {
        var now = DateTime.UtcNow;
        var y = year ?? now.Year;
        var m = month ?? now.Month;

        if (m < 1 || m > 12) return BadRequest("Month must be between 1 and 12.");

        var result = await _analyticsService.GetMonthlyAnalyticsAsync(UserId, y, m);
        return Ok(result);
    }

    /// <summary>Yearly analytics breakdown, defaulting to the current year.</summary>
    [HttpGet("yearly")]
    public async Task<IActionResult> GetYearly([FromQuery] int? year)
    {
        var result = await _analyticsService.GetYearlyAnalyticsAsync(UserId, year ?? DateTime.UtcNow.Year);
        return Ok(result);
    }

    /// <summary>6-month rolling spending trend (for the line chart).</summary>
    [HttpGet("trends")]
    public async Task<IActionResult> GetTrends()
    {
        var result = await _analyticsService.GetTrendsAsync(UserId);
        return Ok(result);
    }
}
