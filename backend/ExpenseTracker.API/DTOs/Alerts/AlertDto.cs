namespace ExpenseTracker.API.DTOs.Alerts;

/// <summary>Alert data returned to the frontend.</summary>
public class AlertDto
{
    public int Id { get; set; }
    public int? TransactionId { get; set; }

    /// <summary>
    /// Alert type string — the frontend uses this to pick icon and color.
    /// Values: "BudgetWarning", "BudgetExceeded", "Anomaly", "NewMerchant"
    /// </summary>
    public string Type { get; set; } = string.Empty;

    public string Message { get; set; } = string.Empty;
    public bool IsRead { get; set; }
    public DateTime CreatedAt { get; set; }
}
