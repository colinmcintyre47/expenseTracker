namespace ExpenseTracker.API.Models;

/// <summary>
/// Tracks every CSV file that has been uploaded by a user.
/// Used for audit trail and to link transactions back to their source file.
/// </summary>
public class UploadedStatement
{
    public int Id { get; set; }
    public int UserId { get; set; }

    /// <summary>Name of the bank this statement came from (e.g., "PNC").</summary>
    public string BankName { get; set; } = string.Empty;

    /// <summary>Original file name as uploaded by the user.</summary>
    public string FileName { get; set; } = string.Empty;

    public DateTime UploadedAt { get; set; } = DateTime.UtcNow;

    /// <summary>How many transactions were imported from this file.</summary>
    public int TransactionCount { get; set; } = 0;

    /// <summary>
    /// Processing status: "Processing", "Completed", "Failed".
    /// Allows the frontend to show progress for large imports.
    /// </summary>
    public string Status { get; set; } = "Processing";

    // --- Navigation ---
    public User User { get; set; } = null!;
    public ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
}
