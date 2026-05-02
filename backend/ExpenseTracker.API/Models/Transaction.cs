namespace ExpenseTracker.API.Models;

/// <summary>
/// A single financial transaction imported from a bank statement.
/// This is the central entity of the entire system.
/// </summary>
public class Transaction
{
    public int Id { get; set; }
    public int UserId { get; set; }

    /// <summary>Category assigned by the categorization engine or manually by the user.</summary>
    public int CategoryId { get; set; }

    /// <summary>Which import batch this transaction came from. Nullable for manual entries.</summary>
    public int? StatementId { get; set; }

    /// <summary>The date the transaction occurred (not the import date).</summary>
    public DateTime Date { get; set; }

    /// <summary>
    /// Cleaned merchant name (e.g., "STARBUCKS #12345" → "Starbucks").
    /// The raw description is preserved separately.
    /// </summary>
    public string Merchant { get; set; } = string.Empty;

    /// <summary>Full raw description from the bank CSV, preserved for reference.</summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Transaction amount in dollars.
    /// Convention: positive = debit (money leaving), negative = credit (money coming in).
    /// This makes math simpler — summing debits = total spent.
    /// </summary>
    public decimal Amount { get; set; }

    /// <summary>"Debit" (expense) or "Credit" (income/refund).</summary>
    public string TransactionType { get; set; } = "Debit";

    /// <summary>Bank account name (e.g., "VIRTUAL WALLET", "CHECKING").</summary>
    public string AccountName { get; set; } = string.Empty;

    /// <summary>
    /// Set to true by the anomaly detection service when a transaction
    /// looks unusual (large amount, new merchant, spending spike).
    /// </summary>
    public bool IsAnomaly { get; set; } = false;

    /// <summary>
    /// SHA-256 hash of (userId + date + amount + description).
    /// Used to detect and reject duplicate CSV uploads.
    /// Stored as a unique constraint in the database.
    /// </summary>
    public string ImportHash { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // --- Navigation ---
    public User User { get; set; } = null!;
    public Category Category { get; set; } = null!;
    public UploadedStatement? Statement { get; set; }
    public ICollection<Alert> Alerts { get; set; } = new List<Alert>();
}
