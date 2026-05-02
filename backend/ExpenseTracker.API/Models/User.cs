namespace ExpenseTracker.API.Models;

/// <summary>
/// Represents an application user.
/// Passwords are stored as BCrypt hashes — never plain text.
/// </summary>
public class User
{
    public int Id { get; set; }

    /// <summary>Email is the unique login identifier.</summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>BCrypt hash of the user's password. Never store plain text.</summary>
    public string PasswordHash { get; set; } = string.Empty;

    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // --- Navigation properties (EF Core uses these to build JOINs) ---
    public ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
    public ICollection<Category> Categories { get; set; } = new List<Category>();
    public ICollection<Budget> Budgets { get; set; } = new List<Budget>();
    public ICollection<Alert> Alerts { get; set; } = new List<Alert>();
    public ICollection<UploadedStatement> UploadedStatements { get; set; } = new List<UploadedStatement>();
}
