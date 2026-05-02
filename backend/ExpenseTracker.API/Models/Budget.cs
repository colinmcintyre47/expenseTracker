namespace ExpenseTracker.API.Models;

/// <summary>
/// A monthly spending limit set by the user for a specific category.
/// Example: User sets Food budget to $500 for March 2025.
/// The budget service compares actual spending against this limit and fires alerts.
/// </summary>
public class Budget
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public int CategoryId { get; set; }

    /// <summary>Maximum allowed spending for this category in the given month.</summary>
    public decimal MonthlyLimit { get; set; }

    /// <summary>Calendar month (1 = January, 12 = December).</summary>
    public int Month { get; set; }

    /// <summary>Calendar year (e.g., 2025).</summary>
    public int Year { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // --- Navigation ---
    public User User { get; set; } = null!;
    public Category Category { get; set; } = null!;
}
