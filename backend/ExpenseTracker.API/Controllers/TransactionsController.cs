using ExpenseTracker.API.DTOs.Transactions;
using ExpenseTracker.API.Services;
using Microsoft.AspNetCore.Mvc;

namespace ExpenseTracker.API.Controllers;

/// <summary>
/// CRUD operations for transactions.
/// All endpoints are authenticated — users can only access their own transactions.
/// </summary>
[Route("api/transactions")]
public class TransactionsController : BaseAuthController
{
    private readonly ITransactionService _transactionService;

    public TransactionsController(ITransactionService transactionService)
    {
        _transactionService = transactionService;
    }

    /// <summary>
    /// Get a paginated, filtered list of transactions.
    /// All filter parameters are optional.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetTransactions([FromQuery] TransactionFilterDto filter)
    {
        var result = await _transactionService.GetTransactionsAsync(UserId, filter);
        return Ok(result);
    }

    /// <summary>Get a single transaction by ID.</summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetTransaction(int id)
    {
        var transaction = await _transactionService.GetByIdAsync(id, UserId);
        if (transaction == null) return NotFound();
        return Ok(transaction);
    }

    /// <summary>Re-categorize a transaction (user-driven correction).</summary>
    [HttpPut("{id}/category")]
    public async Task<IActionResult> UpdateCategory(int id, [FromBody] UpdateTransactionCategoryRequest request)
    {
        var updated = await _transactionService.UpdateCategoryAsync(id, UserId, request.CategoryId);
        return Ok(updated);
    }

    /// <summary>Delete a transaction.</summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteTransaction(int id)
    {
        await _transactionService.DeleteAsync(id, UserId);
        return NoContent();
    }
}
