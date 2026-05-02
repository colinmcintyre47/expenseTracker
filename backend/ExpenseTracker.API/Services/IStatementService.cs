using ExpenseTracker.API.DTOs.Statements;

namespace ExpenseTracker.API.Services;

public interface IStatementService
{
    Task<UploadResultDto> ProcessUploadAsync(Stream fileStream, string fileName, string bankName, int userId, int? statementYear = null);
    Task<List<UploadedStatementDto>> GetStatementsAsync(int userId);
}
