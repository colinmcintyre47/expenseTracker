using ExpenseTracker.API.DTOs.Transactions;
using ExpenseTracker.API.Models;
using ExpenseTracker.API.Repositories;

namespace ExpenseTracker.API.Services;

/// <summary>
/// Business logic for transaction retrieval, filtering, and management.
/// Most of the heavy lifting is in TransactionRepository's filtered query.
/// </summary>
public class TransactionService : ITransactionService
{
    private readonly ITransactionRepository _transactionRepo;

    public TransactionService(ITransactionRepository transactionRepo)
    {
        _transactionRepo = transactionRepo;
    }

    public async Task<PagedTransactionResult> GetTransactionsAsync(int userId, TransactionFilterDto filter)
    {
        // Clamp page size to prevent abusive requests
        filter.PageSize = Math.Clamp(filter.PageSize, 1, 200);

        var (items, total) = await _transactionRepo.GetFilteredAsync(
            userId,
            filter.StartDate, filter.EndDate,
            filter.CategoryId, filter.MinAmount, filter.MaxAmount,
            filter.Search, filter.Page, filter.PageSize,
            filter.SortBy, filter.SortDir);

        return new PagedTransactionResult
        {
            Data = items.Select(MapToDto).ToList(),
            TotalCount = total,
            Page = filter.Page,
            PageSize = filter.PageSize
        };
    }

    public async Task<TransactionDto?> GetByIdAsync(int id, int userId)
    {
        var transaction = await _transactionRepo.GetByIdAsync(id, userId);
        return transaction == null ? null : MapToDto(transaction);
    }

    public async Task<TransactionDto> UpdateCategoryAsync(int id, int userId, int categoryId)
    {
        await _transactionRepo.UpdateCategoryAsync(id, userId, categoryId);
        var updated = await _transactionRepo.GetByIdAsync(id, userId)
            ?? throw new KeyNotFoundException("Transaction not found.");
        return MapToDto(updated);
    }

    public async Task DeleteAsync(int id, int userId)
    {
        await _transactionRepo.DeleteAsync(id, userId);
    }

    private static TransactionDto MapToDto(Transaction t) => new()
    {
        Id = t.Id,
        Date = t.Date,
        Merchant = t.Merchant,
        Description = t.Description,
        Amount = t.Amount,
        TransactionType = t.TransactionType,
        AccountName = t.AccountName,
        IsAnomaly = t.IsAnomaly,
        CreatedAt = t.CreatedAt,
        CategoryId = t.CategoryId,
        CategoryName = t.Category?.Name ?? string.Empty,
        CategoryColor = t.Category?.Color ?? "#6B7280",
        CategoryIcon = t.Category?.Icon ?? "tag"
    };
}
