namespace ExpenseTracker.API.DTOs.Statements;

/// <summary>Uploaded statement summary returned in the statements list.</summary>
public class UploadedStatementDto
{
    public int Id { get; set; }
    public string BankName { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public DateTime UploadedAt { get; set; }
    public int TransactionCount { get; set; }
    public string Status { get; set; } = string.Empty;
}

/// <summary>Response returned after a successful CSV upload.</summary>
public class UploadResultDto
{
    public int StatementId { get; set; }
    public int ImportedCount { get; set; }
    public int DuplicateCount { get; set; }
    public int ErrorCount { get; set; }
    public List<string> Errors { get; set; } = new();
}
