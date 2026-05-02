using System.Globalization;
using CsvHelper;
using CsvHelper.Configuration;

namespace ExpenseTracker.API.Parsers;

/// <summary>
/// Parses PNC Bank CSV statement files.
///
/// PNC CSV FORMAT (Virtual Wallet / Checking accounts):
/// The file has a variable number of header rows before the actual data.
/// PNC exports two types of CSVs:
///
/// Type 1 — Standard Account Activity:
///   Date,Description,Withdrawals,Deposits,Balance
///   01/15/2025,STARBUCKS #12345,-4.75,,1234.56
///
/// Type 2 — Virtual Wallet with multiple columns:
///   Date,Description,Type,Withdrawals,Deposits,Running Balance
///
/// This parser handles both formats by detecting the column headers.
///
/// SAMPLE CSV (included in docs/sample-pnc.csv for testing):
///   Date,Description,Withdrawals,Deposits,Balance
///   01/15/2025,"STARBUCKS #12345 PITTSBURGH PA",-4.75,,1234.56
///   01/14/2025,"AMAZON.COM*12345",-52.99,,1239.31
///   01/13/2025,"DIRECT DEPOSIT EMPLOYER",,2500.00,1292.30
/// </summary>
public class PncBankParser : IBankStatementParser
{
    public string BankName => "PNC";

    public async Task<ParseResult> ParseAsync(Stream csvStream, int? statementYear = null)
    {
        var result = new ParseResult();

        try
        {
            // Reset stream position in case it was read before
            if (csvStream.CanSeek)
                csvStream.Seek(0, SeekOrigin.Begin);

            using var reader = new StreamReader(csvStream);

            // PNC files sometimes have a multi-line header block before the CSV data.
            // We read lines until we find the actual column header row.
            string? rawCsv = await SkipToDataHeaderAsync(reader);

            if (rawCsv == null)
            {
                result.Errors.Add("Could not find the data header row in the PNC CSV file. " +
                    "Expected a row starting with 'Date'.");
                return result;
            }

            // Detect which PNC format we have based on column names
            var format = DetectFormat(rawCsv);

            // Re-combine the header with remaining lines for CsvHelper
            var remaining = await reader.ReadToEndAsync();
            var fullCsv = rawCsv + "\n" + remaining;

            using var stringReader = new StringReader(fullCsv);
            using var csv = new CsvReader(stringReader, new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = true,
                MissingFieldFound = null,          // Don't throw on missing optional fields
                BadDataFound = context =>           // Log bad rows but continue
                {
                    result.Errors.Add($"Skipped bad CSV row: {context.RawRecord?.Trim()}");
                },
                TrimOptions = TrimOptions.Trim,
            });

            await csv.ReadAsync();
            csv.ReadHeader();

            int rowNumber = 1;
            while (await csv.ReadAsync())
            {
                rowNumber++;
                try
                {
                    var row = ParseRow(csv, format);
                    if (row != null)
                        result.Rows.Add(row);
                }
                catch (Exception ex)
                {
                    result.Errors.Add($"Row {rowNumber}: {ex.Message}");
                }
            }
        }
        catch (Exception ex)
        {
            result.Errors.Add($"Failed to parse CSV: {ex.Message}");
        }

        return result;
    }

    // -------------------------------------------------------------------
    // Private helpers
    // -------------------------------------------------------------------

    private enum PncFormat { Standard, VirtualWallet }

    /// <summary>
    /// Reads past any bank metadata at the top of the file,
    /// returning the line that contains the column headers.
    /// </summary>
    private async Task<string?> SkipToDataHeaderAsync(StreamReader reader)
    {
        while (!reader.EndOfStream)
        {
            var line = await reader.ReadLineAsync();
            if (line == null) break;

            // The data starts with a "Date" column header
            if (line.TrimStart().StartsWith("Date", StringComparison.OrdinalIgnoreCase))
                return line;
        }
        return null;
    }

    /// <summary>Detects whether this is a Standard or VirtualWallet CSV.</summary>
    private PncFormat DetectFormat(string headerLine)
    {
        return headerLine.Contains("Type", StringComparison.OrdinalIgnoreCase)
            ? PncFormat.VirtualWallet
            : PncFormat.Standard;
    }

    /// <summary>
    /// Parses a single CSV row into a ParsedTransactionRow.
    /// Returns null for rows that should be skipped (e.g., running balance rows).
    /// </summary>
    private ParsedTransactionRow? ParseRow(CsvReader csv, PncFormat format)
    {
        // --- Date ---
        var dateStr = csv.GetField("Date")?.Trim();
        if (string.IsNullOrEmpty(dateStr)) return null;

        if (!DateTime.TryParseExact(dateStr, new[] { "MM/dd/yyyy", "M/d/yyyy", "yyyy-MM-dd" },
            CultureInfo.InvariantCulture, DateTimeStyles.None, out var date))
        {
            throw new FormatException($"Cannot parse date '{dateStr}'");
        }

        // --- Description ---
        var description = csv.GetField("Description")?.Trim() ?? string.Empty;
        if (string.IsNullOrEmpty(description)) return null;

        // --- Amount + Type ---
        // PNC uses separate Withdrawals and Deposits columns.
        // Withdrawals are negative numbers (money leaving) — we store as positive debits.
        // Deposits are positive numbers (money coming in) — we store as positive credits.
        decimal amount;
        string transactionType;

        var withdrawalStr = csv.GetField("Withdrawals")?.Trim();
        var depositStr = csv.GetField("Deposits")?.Trim();

        if (!string.IsNullOrEmpty(withdrawalStr) &&
            decimal.TryParse(withdrawalStr.Replace("$", "").Replace(",", ""),
                NumberStyles.Any, CultureInfo.InvariantCulture, out var withdrawal) &&
            withdrawal != 0)
        {
            amount = Math.Abs(withdrawal); // Store as positive
            transactionType = "Debit";
        }
        else if (!string.IsNullOrEmpty(depositStr) &&
            decimal.TryParse(depositStr.Replace("$", "").Replace(",", ""),
                NumberStyles.Any, CultureInfo.InvariantCulture, out var deposit) &&
            deposit != 0)
        {
            amount = Math.Abs(deposit);
            transactionType = "Credit";
        }
        else
        {
            // Both columns empty or zero — skip this row (e.g., header continuation rows)
            return null;
        }

        // --- Account Name (VirtualWallet format includes Type column) ---
        var accountName = format == PncFormat.VirtualWallet
            ? csv.GetField("Type")?.Trim() ?? "Checking"
            : "Checking";

        return new ParsedTransactionRow
        {
            Date = date,
            Description = description,
            Amount = amount,
            TransactionType = transactionType,
            AccountName = accountName
        };
    }
}
