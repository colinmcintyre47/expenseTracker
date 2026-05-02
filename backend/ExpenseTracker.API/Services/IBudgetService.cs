using ExpenseTracker.API.DTOs.Budgets;

namespace ExpenseTracker.API.Services;

public interface IBudgetService
{
    Task<List<BudgetDto>> GetBudgetsAsync(int userId, int year, int month);
    Task<BudgetDto> CreateBudgetAsync(int userId, CreateBudgetRequest request);
    Task<BudgetDto> UpdateBudgetAsync(int userId, int budgetId, UpdateBudgetRequest request);
    Task DeleteBudgetAsync(int userId, int budgetId);

    /// <summary>
    /// Called after every debit transaction import.
    /// Checks if the user's spending has crossed 80% or 100% of their budget
    /// and creates an Alert if so.
    /// </summary>
    Task CheckAndCreateBudgetAlertsAsync(int userId, int categoryId, DateTime transactionDate);
}
