using ExpenseTracker.API.Models;

namespace ExpenseTracker.API.Repositories;

public interface IStatementRepository
{
    Task<List<UploadedStatement>> GetForUserAsync(int userId);
    Task<UploadedStatement?> GetByIdAsync(int id, int userId);
    Task<UploadedStatement> CreateAsync(UploadedStatement statement);
    Task UpdateAsync(UploadedStatement statement);
}
