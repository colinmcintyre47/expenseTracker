using ExpenseTracker.API.DTOs.Transactions;

namespace ExpenseTracker.API.Services;

public interface ITransactionService
{
    Task<PagedTransactionResult> GetTransactionsAsync(int userId, TransactionFilterDto filter);
    Task<TransactionDto?> GetByIdAsync(int id, int userId);
    Task<TransactionDto> UpdateCategoryAsync(int id, int userId, int categoryId);
    Task DeleteAsync(int id, int userId);
}
