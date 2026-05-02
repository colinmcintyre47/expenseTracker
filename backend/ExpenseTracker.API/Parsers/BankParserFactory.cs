namespace ExpenseTracker.API.Parsers;

/// <summary>
/// Factory that selects the correct bank parser based on bank name AND file type.
///
/// Each bank can have both a CSV parser and a PDF parser.
/// The factory picks the right one based on the file extension.
///
/// HOW TO ADD A NEW BANK:
/// 1. Create your parser class implementing IBankStatementParser
/// 2. Add it to the _parsers list in the constructor
/// That's it.
/// </summary>
public class BankParserFactory
{
    private readonly List<IBankStatementParser> _parsers;

    public BankParserFactory()
    {
        _parsers = new List<IBankStatementParser>
        {
            new PncBankParser(),    // PNC CSV files
            new PncPdfParser(),     // PNC PDF statements
            // new ChaseParser(),
        };
    }

    /// <summary>
    /// Selects the best parser for the given bank + file extension combination.
    /// If multiple parsers share the same BankName, the one whose file type
    /// matches the extension is preferred.
    /// </summary>
    /// <param name="bankName">e.g. "PNC"</param>
    /// <param name="fileExtension">e.g. ".pdf" or ".csv"</param>
    public IBankStatementParser GetParser(string bankName, string fileExtension)
    {
        var ext = fileExtension.ToLowerInvariant();

        // Try to find a parser whose name matches AND whose class name indicates the right format
        // Convention: CSV parsers end in "BankParser", PDF parsers end in "PdfParser"
        IBankStatementParser? best = null;

        foreach (var parser in _parsers)
        {
            if (!parser.BankName.Equals(bankName, StringComparison.OrdinalIgnoreCase))
                continue;

            var className = parser.GetType().Name.ToLower();
            if (ext == ".pdf" && className.Contains("pdf"))
            {
                best = parser;
                break; // Exact format match — stop looking
            }
            if (ext == ".csv" && !className.Contains("pdf"))
            {
                best = parser;
                break;
            }
            best ??= parser; // Fallback: first matching bank name
        }

        if (best == null)
            throw new NotSupportedException(
                $"No parser found for bank '{bankName}' with file type '{ext}'. " +
                $"Supported banks: {string.Join(", ", GetSupportedBanks())}");

        return best;
    }

    /// <summary>Returns distinct bank names for the upload dropdown.</summary>
    public IEnumerable<string> GetSupportedBanks()
        => _parsers.Select(p => p.BankName).Distinct();
}
