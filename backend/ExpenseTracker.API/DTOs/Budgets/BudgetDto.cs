using System.ComponentModel.DataAnnotations;

namespace ExpenseTracker.API.DTOs.Budgets;

/// <summary>Budget data returned to the frontend, enriched with current spending info.</summary>
public class BudgetDto
{
    public int Id { get; set; }
    public int CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public string CategoryColor { get; set; } = string.Empty;
    public string CategoryIcon { get; set; } = string.Empty;
    public decimal MonthlyLimit { get; set; }
    public int Month { get; set; }
    public int Year { get; set; }

    /// <summary>How much the user has actually spent in this category this month.</summary>
    public decimal AmountSpent { get; set; }

    /// <summary>Remaining budget (MonthlyLimit - AmountSpent). Can be negative if exceeded.</summary>
    public decimal Remaining => MonthlyLimit - AmountSpent;

    /// <summary>Percentage of budget used (0-100+). Over 100 means exceeded.</summary>
    public decimal PercentageUsed => MonthlyLimit > 0 ? (AmountSpent / MonthlyLimit) * 100 : 0;
}

/// <summary>Request body for POST /api/budgets.</summary>
public class CreateBudgetRequest
{
    [Required]
    public int CategoryId { get; set; }

    [Required]
    [Range(0.01, 1_000_000, ErrorMessage = "Limit must be between $0.01 and $1,000,000")]
    public decimal MonthlyLimit { get; set; }

    [Required]
    [Range(1, 12)]
    public int Month { get; set; }

    [Required]
    [Range(2000, 2100)]
    public int Year { get; set; }
}

/// <summary>Request body for PUT /api/budgets/{id}.</summary>
public class UpdateBudgetRequest
{
    [Required]
    [Range(0.01, 1_000_000)]
    public decimal MonthlyLimit { get; set; }
}
