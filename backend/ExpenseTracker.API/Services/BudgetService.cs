using ExpenseTracker.API.DTOs.Budgets;
using ExpenseTracker.API.Models;
using ExpenseTracker.API.Repositories;

namespace ExpenseTracker.API.Services;

/// <summary>
/// Manages monthly budgets and fires alerts when thresholds are crossed.
///
/// Alert thresholds:
/// - 80% used → BudgetWarning alert ("Approaching your Food budget")
/// - 100%+ used → BudgetExceeded alert ("You've exceeded your Food budget")
///
/// Duplicate alert prevention:
/// We don't create a new alert if one of the same type already exists for this
/// category+month, to avoid spamming the user on every transaction.
/// </summary>
public class BudgetService : IBudgetService
{
    private readonly IBudgetRepository _budgetRepo;
    private readonly ITransactionRepository _transactionRepo;
    private readonly IAlertRepository _alertRepo;
    private readonly ICategoryRepository _categoryRepo;

    public BudgetService(
        IBudgetRepository budgetRepo,
        ITransactionRepository transactionRepo,
        IAlertRepository alertRepo,
        ICategoryRepository categoryRepo)
    {
        _budgetRepo = budgetRepo;
        _transactionRepo = transactionRepo;
        _alertRepo = alertRepo;
        _categoryRepo = categoryRepo;
    }

    public async Task<List<BudgetDto>> GetBudgetsAsync(int userId, int year, int month)
    {
        var budgets = await _budgetRepo.GetForUserMonthAsync(userId, year, month);

        // For each budget, look up actual spending for that category this month
        var result = new List<BudgetDto>();
        foreach (var budget in budgets)
        {
            var transactions = await _transactionRepo.GetByMonthAsync(userId, year, month);
            var spent = transactions
                .Where(t => t.CategoryId == budget.CategoryId && t.TransactionType == "Debit")
                .Sum(t => t.Amount);

            result.Add(MapToDto(budget, spent));
        }

        return result.OrderBy(b => b.CategoryName).ToList();
    }

    public async Task<BudgetDto> CreateBudgetAsync(int userId, CreateBudgetRequest request)
    {
        // Check for duplicate budget (same category/month/year)
        var existing = await _budgetRepo.GetByCategoryMonthAsync(
            userId, request.CategoryId, request.Year, request.Month);

        if (existing != null)
            throw new InvalidOperationException(
                "A budget for this category and month already exists. Use PUT to update it.");

        var budget = new Budget
        {
            UserId = userId,
            CategoryId = request.CategoryId,
            MonthlyLimit = request.MonthlyLimit,
            Month = request.Month,
            Year = request.Year
        };

        var created = await _budgetRepo.CreateAsync(budget);

        // Reload with category navigation property
        var reloaded = await _budgetRepo.GetByIdAsync(created.Id, userId);
        return MapToDto(reloaded!, 0);
    }

    public async Task<BudgetDto> UpdateBudgetAsync(int userId, int budgetId, UpdateBudgetRequest request)
    {
        var budget = await _budgetRepo.GetByIdAsync(budgetId, userId)
            ?? throw new KeyNotFoundException("Budget not found.");

        budget.MonthlyLimit = request.MonthlyLimit;
        await _budgetRepo.UpdateAsync(budget);

        // Calculate current spending for the response
        var transactions = await _transactionRepo.GetByMonthAsync(userId, budget.Year, budget.Month);
        var spent = transactions
            .Where(t => t.CategoryId == budget.CategoryId && t.TransactionType == "Debit")
            .Sum(t => t.Amount);

        return MapToDto(budget, spent);
    }

    public async Task DeleteBudgetAsync(int userId, int budgetId)
    {
        await _budgetRepo.DeleteAsync(budgetId, userId);
    }

    public async Task CheckAndCreateBudgetAlertsAsync(int userId, int categoryId, DateTime transactionDate)
    {
        // Find the budget for this category + month
        var budget = await _budgetRepo.GetByCategoryMonthAsync(
            userId, categoryId, transactionDate.Year, transactionDate.Month);

        if (budget == null) return; // No budget set for this category

        // Calculate total spending so far this month in this category
        var transactions = await _transactionRepo.GetByMonthAsync(
            userId, transactionDate.Year, transactionDate.Month);
        var totalSpent = transactions
            .Where(t => t.CategoryId == categoryId && t.TransactionType == "Debit")
            .Sum(t => t.Amount);

        var percentage = budget.MonthlyLimit > 0
            ? (totalSpent / budget.MonthlyLimit) * 100
            : 0;

        var category = await _categoryRepo.GetByIdAsync(categoryId);
        var categoryName = category?.Name ?? "Unknown";

        // Check thresholds and create alerts (avoid duplicates by checking existing alerts)
        var existingAlerts = await _alertRepo.GetForUserAsync(userId);

        if (percentage >= 100)
        {
            // Check if we already have a BudgetExceeded alert for this category this month
            var alreadyAlerted = existingAlerts.Any(a =>
                a.Type == "BudgetExceeded" &&
                a.CreatedAt.Month == transactionDate.Month &&
                a.CreatedAt.Year == transactionDate.Year &&
                a.Message.Contains(categoryName));

            if (!alreadyAlerted)
            {
                await _alertRepo.CreateAsync(new Alert
                {
                    UserId = userId,
                    Type = "BudgetExceeded",
                    Message = $"You've exceeded your {categoryName} budget! " +
                              $"Spent ${totalSpent:F2} of ${budget.MonthlyLimit:F2} ({percentage:F0}%)"
                });
            }
        }
        else if (percentage >= 80)
        {
            var alreadyAlerted = existingAlerts.Any(a =>
                a.Type == "BudgetWarning" &&
                a.CreatedAt.Month == transactionDate.Month &&
                a.CreatedAt.Year == transactionDate.Year &&
                a.Message.Contains(categoryName));

            if (!alreadyAlerted)
            {
                await _alertRepo.CreateAsync(new Alert
                {
                    UserId = userId,
                    Type = "BudgetWarning",
                    Message = $"Approaching your {categoryName} budget: " +
                              $"${totalSpent:F2} of ${budget.MonthlyLimit:F2} used ({percentage:F0}%)"
                });
            }
        }
    }

    private static BudgetDto MapToDto(Budget budget, decimal amountSpent) => new()
    {
        Id = budget.Id,
        CategoryId = budget.CategoryId,
        CategoryName = budget.Category?.Name ?? string.Empty,
        CategoryColor = budget.Category?.Color ?? "#6B7280",
        CategoryIcon = budget.Category?.Icon ?? "tag",
        MonthlyLimit = budget.MonthlyLimit,
        Month = budget.Month,
        Year = budget.Year,
        AmountSpent = amountSpent
    };
}
