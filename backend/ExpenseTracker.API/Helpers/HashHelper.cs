using System.Security.Cryptography;
using System.Text;

namespace ExpenseTracker.API.Helpers;

/// <summary>
/// Utility for generating deterministic hashes used to detect duplicate transactions.
///
/// How duplicate detection works:
/// When a user uploads a CSV, each row is hashed using a combination of:
///   userId + date + amount + description
/// If that hash already exists in the database (unique constraint), the transaction is skipped.
/// This means the same file can be uploaded multiple times safely.
/// </summary>
public static class HashHelper
{
    /// <summary>
    /// Generates a SHA-256 hash for a transaction row.
    /// This hash is stored in the ImportHash column of the Transactions table.
    /// </summary>
    /// <param name="userId">The user who owns this transaction</param>
    /// <param name="date">Transaction date</param>
    /// <param name="amount">Transaction amount</param>
    /// <param name="description">Raw description from the bank CSV</param>
    /// <returns>Hex-encoded SHA-256 hash string (64 characters)</returns>
    public static string GenerateTransactionHash(int userId, DateTime date, decimal amount, string description)
    {
        // Combine the fields into a stable, canonical string
        // Format is chosen to be unambiguous — colons separate fields
        var input = $"{userId}:{date:yyyy-MM-dd}:{amount:F2}:{description.Trim().ToLowerInvariant()}";

        // Compute SHA-256 and convert to lowercase hex
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(input));
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }
}
