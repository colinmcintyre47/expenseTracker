using ExpenseTracker.API.Models;
using ExpenseTracker.API.Repositories;

namespace ExpenseTracker.API.Services;

/// <summary>
/// Detects unusual transactions using statistical heuristics.
///
/// DETECTION RULES (in priority order):
/// 1. Large purchase: transaction amount > $500 (configurable threshold)
/// 2. New merchant: user has never transacted with this merchant before
/// 3. Spending spike: this transaction is 3x the user's average for this category
///
/// Each rule is independent — a single transaction can trigger multiple anomalies.
/// When an anomaly is detected, an Alert is created via the AlertService.
/// </summary>
public class AnomalyDetectionService : IAnomalyDetectionService
{
    private readonly ITransactionRepository _transactionRepo;

    // Thresholds — could be moved to appsettings.json for runtime configuration
    private const decimal LargePurchaseThreshold = 500m;
    private const decimal SpendingSpikeMultiplier = 3.0m;
    private const int LookbackMonths = 3; // Compare against last 3 months

    public AnomalyDetectionService(ITransactionRepository transactionRepo)
    {
        _transactionRepo = transactionRepo;
    }

    public async Task<List<string>> DetectAnomaliesAsync(Transaction transaction, int userId)
    {
        var anomalies = new List<string>();

        // Only analyze debit transactions (spending), not credits/income
        if (transaction.TransactionType != "Debit") return anomalies;

        // --- Rule 1: Large Purchase ---
        if (transaction.Amount > LargePurchaseThreshold)
        {
            anomalies.Add($"Large purchase: ${transaction.Amount:F2} at {transaction.Merchant}");
        }

        // --- Rule 2: New Merchant ---
        // Check if user has ever transacted with this merchant before (excluding this transaction)
        if (!string.IsNullOrEmpty(transaction.Merchant))
        {
            var history = await _transactionRepo.GetMerchantHistoryAsync(
                userId, transaction.Merchant, limitMonths: 12);

            // If no prior transactions found, this is a new merchant
            if (!history.Any())
            {
                anomalies.Add($"New merchant: first transaction at {transaction.Merchant}");
            }
        }

        // --- Rule 3: Spending Spike ---
        // Compare this transaction to the user's average monthly spend in this category
        var avgMonthlySpend = await _transactionRepo.GetAverageMonthlySpendingAsync(
            userId, transaction.CategoryId, LookbackMonths);

        if (avgMonthlySpend > 0 && transaction.Amount > avgMonthlySpend * SpendingSpikeMultiplier)
        {
            anomalies.Add(
                $"Spending spike: ${transaction.Amount:F2} is {SpendingSpikeMultiplier}x the " +
                $"monthly average of ${avgMonthlySpend:F2} in this category");
        }

        return anomalies;
    }
}
