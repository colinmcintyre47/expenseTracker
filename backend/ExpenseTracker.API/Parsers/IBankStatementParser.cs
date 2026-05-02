namespace ExpenseTracker.API.Parsers;

/// <summary>
/// Interface for bank-specific CSV statement parsers.
///
/// HOW TO ADD A NEW BANK:
/// 1. Create a new class in this folder: e.g., "ChaseParser.cs"
/// 2. Implement this interface (ParseAsync method)
/// 3. Register the parser in BankParserFactory.cs
/// That's it — no other code changes needed.
///
/// The IBankStatementParser abstraction means the rest of the system
/// (StatementService, controllers) doesn't know or care which bank format is being processed.
/// </summary>
public interface IBankStatementParser
{
    /// <summary>Human-readable name used for matching (e.g., "PNC").</summary>
    string BankName { get; }

    /// <summary>
    /// Parses a CSV stream into a list of raw parsed rows.
    /// Returns ParsedTransactionRow objects — not domain models —
    /// so the service layer can apply business logic (categorization, hashing, etc.).
    /// </summary>
    /// <param name="fileStream">The uploaded file stream</param>
    /// <param name="statementYear">Optional year override — use when the file doesn't embed the year (common in PDFs)</param>
    /// <returns>List of parsed rows + any parse errors encountered</returns>
    Task<ParseResult> ParseAsync(Stream fileStream, int? statementYear = null);
}

/// <summary>A single transaction row parsed from a bank CSV file.</summary>
public class ParsedTransactionRow
{
    public DateTime Date { get; set; }
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Raw amount from the CSV — may be negative for credits.
    /// The parser normalizes so debits are positive.
    /// </summary>
    public decimal Amount { get; set; }
    public string TransactionType { get; set; } = "Debit"; // "Debit" or "Credit"
    public string AccountName { get; set; } = string.Empty;
}

/// <summary>Result of parsing a CSV file, including any non-fatal errors.</summary>
public class ParseResult
{
    public List<ParsedTransactionRow> Rows { get; set; } = new();
    public List<string> Errors { get; set; } = new();
    public bool Success => Rows.Count > 0 || !Errors.Any();
}
