using ExpenseTracker.API.DTOs.Budgets;
using ExpenseTracker.API.Services;
using Microsoft.AspNetCore.Mvc;

namespace ExpenseTracker.API.Controllers;

/// <summary>
/// Budget CRUD endpoints.
/// Budgets are scoped per user / category / month / year.
/// </summary>
[Route("api/budgets")]
public class BudgetsController : BaseAuthController
{
    private readonly IBudgetService _budgetService;

    public BudgetsController(IBudgetService budgetService)
    {
        _budgetService = budgetService;
    }

    /// <summary>
    /// Get all budgets for the given month.
    /// Each budget includes current spending so the frontend can show progress bars.
    /// Defaults to current month/year.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetBudgets([FromQuery] int? year, [FromQuery] int? month)
    {
        var now = DateTime.UtcNow;
        var result = await _budgetService.GetBudgetsAsync(
            UserId, year ?? now.Year, month ?? now.Month);
        return Ok(result);
    }

    /// <summary>Create a new monthly budget for a category.</summary>
    [HttpPost]
    public async Task<IActionResult> CreateBudget([FromBody] CreateBudgetRequest request)
    {
        var result = await _budgetService.CreateBudgetAsync(UserId, request);
        return StatusCode(201, result);
    }

    /// <summary>Update an existing budget's monthly limit.</summary>
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateBudget(int id, [FromBody] UpdateBudgetRequest request)
    {
        var result = await _budgetService.UpdateBudgetAsync(UserId, id, request);
        return Ok(result);
    }

    /// <summary>Delete a budget.</summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteBudget(int id)
    {
        await _budgetService.DeleteBudgetAsync(UserId, id);
        return NoContent();
    }
}
