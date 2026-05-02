using ExpenseTracker.API.Models;

namespace ExpenseTracker.API.Services;

/// <summary>
/// Interface for detecting unusual spending patterns.
/// Called after each transaction import to flag suspicious transactions.
/// </summary>
public interface IAnomalyDetectionService
{
    /// <summary>
    /// Analyzes a transaction and returns a list of anomaly descriptions if any are found.
    /// Returns an empty list if the transaction looks normal.
    /// </summary>
    Task<List<string>> DetectAnomaliesAsync(Transaction transaction, int userId);
}
