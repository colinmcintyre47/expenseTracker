namespace ExpenseTracker.API.Models;

/// <summary>
/// A spending category (e.g., "Food", "Transportation").
/// System categories (IsSystem = true) are seeded on startup and shared across all users.
/// User-created categories have UserId set and are private to that user.
/// </summary>
public class Category
{
    public int Id { get; set; }

    /// <summary>
    /// NULL for system/default categories that are available to all users.
    /// Set to a UserId for custom categories created by a specific user.
    /// </summary>
    public int? UserId { get; set; }

    public string Name { get; set; } = string.Empty;

    /// <summary>Hex color code for UI display (e.g., "#FF5733").</summary>
    public string Color { get; set; } = "#6B7280";

    /// <summary>Icon identifier string used by the frontend icon library.</summary>
    public string Icon { get; set; } = "tag";

    /// <summary>System categories are seeded defaults and cannot be deleted by users.</summary>
    public bool IsSystem { get; set; } = false;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // --- Navigation properties ---
    public User? User { get; set; }
    public ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
    public ICollection<Budget> Budgets { get; set; } = new List<Budget>();
}
