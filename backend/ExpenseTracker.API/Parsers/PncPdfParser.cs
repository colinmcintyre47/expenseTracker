using System.Globalization;
using System.Text.RegularExpressions;
using UglyToad.PdfPig;
using UglyToad.PdfPig.Content;

namespace ExpenseTracker.API.Parsers;

/// <summary>
/// Parses PNC Bank PDF statement files.
///
/// PNC PDF STATEMENT STRUCTURE:
/// The PDF is organized into sections by transaction type:
///
///   "Deposits and Other Additions"
///   MM/DD   amount   Description
///
///   "Banking/Debit Card Withdrawals and Purchases"
///   MM/DD   amount   Description
///
///   "Online and Electronic Banking Deductions"
///   MM/DD   amount   Description
///
///   "Other Deductions"
///   MM/DD   amount   Description
///
/// Each transaction line matches the pattern:
///   01/15    4.75    STARBUCKS #12345 PITTSBURGH PA
///
/// Some lines span multiple rows in the PDF — this parser collects
/// continuation lines (no date at start) as part of the previous transaction's description.
///
/// The year is inferred from the statement period header
/// (e.g., "For the period 01/01/2025 to 01/31/2025").
/// </summary>
public class PncPdfParser : IBankStatementParser
{
    public string BankName => "PNC";

    // Matches: MM/DD  amount  description
    // Examples: "01/15  4.75  STARBUCKS #12345"
    //           "03/07  2,500.00  DIRECT DEPOSIT"
    private static readonly Regex TransactionLineRegex = new(
        @"^(\d{2}/\d{2})\s+([\d,]+\.\d{2})\s+(.+)$",
        RegexOptions.Compiled);

    // Matches any 4-digit year in the 2000s (used to find the statement year from the header)
    // Covers formats like: "01/01/2025", "January 2025", "2025", "2025-01-01"
    private static readonly Regex StatementYearRegex = new(
        @"\b(20\d{2})\b",
        RegexOptions.Compiled);

    // Section headers that indicate whether transactions are debits or credits
    private static readonly Dictionary<string, string> SectionTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        { "Deposits and Other Additions",                    "Credit" },
        { "Banking/Debit Card Withdrawals and Purchases",   "Debit" },
        { "Online and Electronic Banking Deductions",        "Debit" },
        { "Other Deductions",                                "Debit" },
        { "Checks Cleared",                                  "Debit" },
        { "Service Charges and Fees",                        "Debit" },
        { "ATM Withdrawals",                                 "Debit" },
    };

    public async Task<ParseResult> ParseAsync(Stream pdfStream, int? statementYear = null)
    {
        var result = new ParseResult();

        try
        {
            // PdfPig needs to read the entire stream
            // Copy to MemoryStream if the stream isn't seekable
            Stream readableStream = pdfStream;
            if (!pdfStream.CanSeek)
            {
                var ms = new MemoryStream();
                await pdfStream.CopyToAsync(ms);
                ms.Seek(0, SeekOrigin.Begin);
                readableStream = ms;
            }

            using var pdf = PdfDocument.Open(readableStream);

            // Extract all text from all pages in order
            var allLines = ExtractLines(pdf);

            // Use caller-supplied year, or try to infer from the PDF text
            int year = statementYear ?? InferYear(allLines);

            // Walk through lines tracking which section we're in
            string currentTransactionType = "Debit"; // default
            ParsedTransactionRow? pendingRow = null;

            foreach (var line in allLines)
            {
                var trimmed = line.Trim();
                if (string.IsNullOrWhiteSpace(trimmed)) continue;

                // Check if this line is a section header
                var sectionType = GetSectionType(trimmed);
                if (sectionType != null)
                {
                    // Flush any pending row before switching sections
                    if (pendingRow != null)
                        result.Rows.Add(pendingRow);
                    pendingRow = null;
                    currentTransactionType = sectionType;
                    continue;
                }

                // Try to match a transaction line: MM/DD  amount  description
                var match = TransactionLineRegex.Match(trimmed);
                if (match.Success)
                {
                    // Flush the previous row
                    if (pendingRow != null)
                        result.Rows.Add(pendingRow);

                    var dateStr = match.Groups[1].Value;           // "01/15"
                    var amountStr = match.Groups[2].Value;          // "4.75" or "2,500.00"
                    var description = match.Groups[3].Value.Trim(); // "STARBUCKS #12345"

                    if (!DateTime.TryParseExact(
                        $"{dateStr}/{year}",
                        "MM/dd/yyyy",
                        CultureInfo.InvariantCulture,
                        DateTimeStyles.None,
                        out var date))
                    {
                        result.Errors.Add($"Could not parse date '{dateStr}' on line: {trimmed}");
                        pendingRow = null;
                        continue;
                    }

                    if (!decimal.TryParse(
                        amountStr.Replace(",", ""),
                        NumberStyles.Any, CultureInfo.InvariantCulture,
                        out var amount))
                    {
                        result.Errors.Add($"Could not parse amount '{amountStr}' on line: {trimmed}");
                        pendingRow = null;
                        continue;
                    }

                    pendingRow = new ParsedTransactionRow
                    {
                        Date = date,
                        Description = description,
                        Amount = amount,
                        TransactionType = currentTransactionType,
                        AccountName = "Checking"
                    };
                }
                else if (pendingRow != null && !IsPageArtifact(trimmed))
                {
                    // This line is a continuation of the previous transaction's description.
                    // PNC PDFs sometimes wrap long merchant names across lines.
                    pendingRow.Description += " " + trimmed;
                }
            }

            // Flush the last pending row
            if (pendingRow != null)
                result.Rows.Add(pendingRow);

            if (result.Rows.Count == 0 && !result.Errors.Any())
            {
                result.Errors.Add(
                    "No transactions found in this PDF. " +
                    "Make sure this is a PNC account activity statement (not a credit card statement).");
            }
        }
        catch (Exception ex)
        {
            result.Errors.Add($"Failed to parse PDF: {ex.Message}");
        }

        return result;
    }

    // -------------------------------------------------------------------
    // Private helpers
    // -------------------------------------------------------------------

    /// <summary>
    /// Extracts all text lines from the PDF, preserving page order.
    /// PdfPig returns words with coordinates — we group them into lines
    /// by their Y position (rounded to the nearest pixel to handle slight offsets).
    /// </summary>
    private static List<string> ExtractLines(PdfDocument pdf)
    {
        var lines = new List<string>();

        foreach (var page in pdf.GetPages())
        {
            // PNC statements have a running balance column on the far right of each row.
            // If we include those words, the balance figure merges with the next transaction's
            // date on the same Y coordinate and gets misread as the transaction amount.
            //
            // Fix: exclude any word whose left edge is in the rightmost 18% of the page.
            // Standard PNC letter-size pages are ~612pt wide; the balance column starts
            // around X=500, which is ~82% from the left.
            double pageWidth = page.Width;
            double balanceColumnCutoff = pageWidth * 0.82;

            var wordsByLine = page.GetWords()
                .Where(w => w.BoundingBox.Left < balanceColumnCutoff)
                .GroupBy(w => (int)Math.Round(w.BoundingBox.Bottom, 0))
                .OrderByDescending(g => g.Key) // Top of page = higher Y in PDF coordinates
                .Select(g => string.Join(" ", g.OrderBy(w => w.BoundingBox.Left).Select(w => w.Text)));

            lines.AddRange(wordsByLine);
        }

        return lines;
    }

    /// <summary>
    /// Searches the extracted text for the statement period header to get the year.
    /// Falls back to the current year if not found.
    /// </summary>
    private static int InferYear(IEnumerable<string> lines)
    {
        // Search all lines for a 4-digit year (20xx).
        // First match wins — PNC statements start with the period header near the top.
        foreach (var line in lines)
        {
            var match = StatementYearRegex.Match(line);
            if (match.Success && int.TryParse(match.Groups[1].Value, out var year))
                return year;
        }
        return DateTime.Now.Year;
    }

    /// <summary>Returns the transaction type for a section header line, or null if not a header.</summary>
    private static string? GetSectionType(string line)
    {
        foreach (var kvp in SectionTypes)
        {
            if (line.Contains(kvp.Key, StringComparison.OrdinalIgnoreCase))
                return kvp.Value;
        }
        return null;
    }

    /// <summary>
    /// Returns true for lines that are PDF artifacts and should not be
    /// treated as transaction description continuations.
    /// Examples: page numbers, "continued", column headers, balance totals.
    /// </summary>
    private static bool IsPageArtifact(string line)
    {
        if (line.Length < 3) return true;

        var lower = line.ToLower();
        return lower.StartsWith("page ") ||
               lower == "continued" ||
               lower.StartsWith("date ") ||
               lower.StartsWith("balance") ||
               lower.StartsWith("total ") ||
               lower.StartsWith("beginning balance") ||
               lower.StartsWith("ending balance") ||
               lower.StartsWith("account number") ||
               Regex.IsMatch(line, @"^\$?[\d,]+\.\d{2}$") || // standalone dollar amounts = balance column
               // Fallback: "MM/DD  large-number  MM/DD" means balance leaked into the transaction area.
               // Real transaction descriptions are not bare dates.
               Regex.IsMatch(line, @"^\d{2}/\d{2}\s+[\d,]+\.\d{2}\s+\d{2}/\d{2}$");
    }
}
