using ExpenseTracker.API.DTOs.Statements;
using ExpenseTracker.API.Helpers;
using ExpenseTracker.API.Models;
using ExpenseTracker.API.Parsers;
using ExpenseTracker.API.Repositories;

namespace ExpenseTracker.API.Services;

/// <summary>
/// Orchestrates the full CSV import pipeline:
/// 1. Parse the CSV using the appropriate bank parser
/// 2. Extract a merchant name from the raw description
/// 3. Categorize each transaction using the categorization service
/// 4. Generate an import hash for duplicate detection
/// 5. Run anomaly detection on each transaction
/// 6. Persist transactions in bulk
/// 7. Check budget thresholds and create alerts
/// </summary>
public class StatementService : IStatementService
{
    private readonly BankParserFactory _parserFactory;
    private readonly ITransactionRepository _transactionRepo;
    private readonly IStatementRepository _statementRepo;
    private readonly ICategoryRepository _categoryRepo;
    private readonly ICategorizationService _categorizationService;
    private readonly IAnomalyDetectionService _anomalyService;
    private readonly IBudgetService _budgetService;
    private readonly IAlertRepository _alertRepo;

    public StatementService(
        BankParserFactory parserFactory,
        ITransactionRepository transactionRepo,
        IStatementRepository statementRepo,
        ICategoryRepository categoryRepo,
        ICategorizationService categorizationService,
        IAnomalyDetectionService anomalyService,
        IBudgetService budgetService,
        IAlertRepository alertRepo)
    {
        _parserFactory = parserFactory;
        _transactionRepo = transactionRepo;
        _statementRepo = statementRepo;
        _categoryRepo = categoryRepo;
        _categorizationService = categorizationService;
        _anomalyService = anomalyService;
        _budgetService = budgetService;
        _alertRepo = alertRepo;
    }

    public async Task<UploadResultDto> ProcessUploadAsync(
        Stream fileStream, string fileName, string bankName, int userId, int? statementYear = null)
    {
        // Create the statement record immediately so we have an ID to reference
        var statement = await _statementRepo.CreateAsync(new UploadedStatement
        {
            UserId = userId,
            BankName = bankName,
            FileName = fileName,
            Status = "Processing"
        });

        var result = new UploadResultDto { StatementId = statement.Id };

        try
        {
            // Step 1: Select parser by bank name and file extension (CSV vs PDF)
            var fileExtension = Path.GetExtension(fileName);
            var parser = _parserFactory.GetParser(bankName, fileExtension);
            var parseResult = await parser.ParseAsync(fileStream, statementYear);

            result.Errors.AddRange(parseResult.Errors);

            if (!parseResult.Rows.Any() && parseResult.Errors.Any())
            {
                statement.Status = "Failed";
                await _statementRepo.UpdateAsync(statement);
                return result;
            }

            // Step 2: Get the "Uncategorized" fallback category
            var uncategorized = await _categoryRepo.GetUncategorizedAsync()
                ?? throw new InvalidOperationException("System category 'Uncategorized' not found. Run seed data.");

            // Step 3: Process each parsed row
            var transactionsToInsert = new List<Transaction>();

            foreach (var row in parseResult.Rows)
            {
                try
                {
                    // Generate duplicate-detection hash
                    var hash = HashHelper.GenerateTransactionHash(userId, row.Date, row.Amount, row.Description);

                    // Skip if this transaction was already imported
                    if (await _transactionRepo.HashExistsAsync(hash))
                    {
                        result.DuplicateCount++;
                        continue;
                    }

                    // Extract a clean merchant name from the raw description
                    var merchant = ExtractMerchantName(row.Description);

                    // Categorize based on description
                    var categoryId = await _categorizationService.CategorizeAsync(
                        row.Description, uncategorized.Id);

                    transactionsToInsert.Add(new Transaction
                    {
                        UserId = userId,
                        StatementId = statement.Id,
                        CategoryId = categoryId,
                        Date = row.Date,
                        Merchant = merchant,
                        Description = row.Description,
                        Amount = row.Amount,
                        TransactionType = row.TransactionType,
                        AccountName = row.AccountName,
                        ImportHash = hash
                    });
                }
                catch (Exception ex)
                {
                    result.ErrorCount++;
                    result.Errors.Add($"Error processing row '{row.Description}': {ex.Message}");
                }
            }

            // Deduplicate within the batch itself (same PDF may have identical rows)
            transactionsToInsert = transactionsToInsert
                .GroupBy(t => t.ImportHash)
                .Select(g => g.First())
                .ToList();

            // Step 4: Bulk insert all valid transactions
            if (transactionsToInsert.Any())
            {
                await _transactionRepo.AddRangeAsync(transactionsToInsert);
                result.ImportedCount = transactionsToInsert.Count;

                // Step 5: Run anomaly detection and create alerts for flagged transactions
                foreach (var transaction in transactionsToInsert)
                {
                    var anomalies = await _anomalyService.DetectAnomaliesAsync(transaction, userId);
                    if (anomalies.Any())
                    {
                        transaction.IsAnomaly = true;
                        foreach (var anomaly in anomalies)
                        {
                            await _alertRepo.CreateAsync(new Alert
                            {
                                UserId = userId,
                                TransactionId = transaction.Id,
                                Type = anomaly.StartsWith("Large") ? "Anomaly"
                                     : anomaly.StartsWith("New") ? "NewMerchant"
                                     : "Anomaly",
                                Message = anomaly
                            });
                        }
                    }

                    // Step 6: Check budgets for debit transactions
                    if (transaction.TransactionType == "Debit")
                    {
                        await _budgetService.CheckAndCreateBudgetAlertsAsync(
                            userId, transaction.CategoryId, transaction.Date);
                    }
                }
            }

            // Update statement record with final counts
            statement.TransactionCount = result.ImportedCount;
            statement.Status = "Completed";
            await _statementRepo.UpdateAsync(statement);
        }
        catch (Exception ex)
        {
            statement.Status = "Failed";
            await _statementRepo.UpdateAsync(statement);
            result.Errors.Add($"Import failed: {ex.Message}");
        }

        return result;
    }

    public async Task<List<UploadedStatementDto>> GetStatementsAsync(int userId)
    {
        var statements = await _statementRepo.GetForUserAsync(userId);
        return statements.Select(s => new UploadedStatementDto
        {
            Id = s.Id,
            BankName = s.BankName,
            FileName = s.FileName,
            UploadedAt = s.UploadedAt,
            TransactionCount = s.TransactionCount,
            Status = s.Status
        }).ToList();
    }

    /// <summary>
    /// Extracts a clean, short merchant name from a raw bank description.
    /// Examples:
    ///   "STARBUCKS #12345 PITTSBURGH PA" → "Starbucks"
    ///   "AMAZON.COM*ABCD1234" → "Amazon.com"
    ///   "DIRECT DEPOSIT EMPLOYER NAME" → "Direct Deposit"
    /// </summary>
    private static string ExtractMerchantName(string description)
    {
        if (string.IsNullOrWhiteSpace(description)) return "Unknown";

        // Remove transaction codes (patterns like *ABCD1234, #12345)
        var cleaned = System.Text.RegularExpressions.Regex.Replace(description, @"[#*]\S+", "").Trim();

        // Remove trailing location info (city/state pattern: "CITY ST" at end)
        cleaned = System.Text.RegularExpressions.Regex.Replace(
            cleaned, @"\s+[A-Z]{2,3}\s+[A-Z]{2}\s*$", "").Trim();

        // Take the first 3 words as the merchant name
        var words = cleaned.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        var merchantWords = words.Take(3);

        // Title-case the result
        var merchant = string.Join(" ", merchantWords);
        return System.Globalization.CultureInfo.CurrentCulture.TextInfo.ToTitleCase(merchant.ToLower());
    }
}
