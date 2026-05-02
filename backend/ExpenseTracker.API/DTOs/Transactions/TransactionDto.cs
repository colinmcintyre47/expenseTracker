namespace ExpenseTracker.API.DTOs.Transactions;

/// <summary>
/// Transaction data returned to the frontend.
/// Uses category name/color directly to avoid extra lookups on the client.
/// </summary>
public class TransactionDto
{
    public int Id { get; set; }
    public DateTime Date { get; set; }
    public string Merchant { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string TransactionType { get; set; } = string.Empty;
    public string AccountName { get; set; } = string.Empty;
    public bool IsAnomaly { get; set; }
    public DateTime CreatedAt { get; set; }

    // Category info — flattened to avoid nested objects in the list view
    public int CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public string CategoryColor { get; set; } = string.Empty;
    public string CategoryIcon { get; set; } = string.Empty;
}

/// <summary>
/// Query parameters for GET /api/transactions.
/// All filters are optional — omitting a filter returns all transactions.
/// </summary>
public class TransactionFilterDto
{
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public int? CategoryId { get; set; }
    public decimal? MinAmount { get; set; }
    public decimal? MaxAmount { get; set; }

    /// <summary>Full-text search against Merchant and Description fields.</summary>
    public string? Search { get; set; }

    /// <summary>Page number (1-based). Default: 1.</summary>
    public int Page { get; set; } = 1;

    /// <summary>Number of results per page. Default: 50, Max: 200.</summary>
    public int PageSize { get; set; } = 50;

    /// <summary>Column to sort by. Options: date, amount, merchant. Default: date.</summary>
    public string SortBy { get; set; } = "date";

    /// <summary>Sort direction: "asc" or "desc". Default: "desc".</summary>
    public string SortDir { get; set; } = "desc";
}

/// <summary>Paginated list response returned by GET /api/transactions.</summary>
public class PagedTransactionResult
{
    public List<TransactionDto> Data { get; set; } = new();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
}

/// <summary>Request body for re-categorizing a transaction.</summary>
public class UpdateTransactionCategoryRequest
{
    public int CategoryId { get; set; }
}
