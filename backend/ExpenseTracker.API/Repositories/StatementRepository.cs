using ExpenseTracker.API.Data;
using ExpenseTracker.API.Models;
using Microsoft.EntityFrameworkCore;

namespace ExpenseTracker.API.Repositories;

public class StatementRepository : IStatementRepository
{
    private readonly AppDbContext _db;
    public StatementRepository(AppDbContext db) => _db = db;

    public async Task<List<UploadedStatement>> GetForUserAsync(int userId)
        => await _db.UploadedStatements
            .Where(s => s.UserId == userId)
            .OrderByDescending(s => s.UploadedAt)
            .ToListAsync();

    public async Task<UploadedStatement?> GetByIdAsync(int id, int userId)
        => await _db.UploadedStatements
            .FirstOrDefaultAsync(s => s.Id == id && s.UserId == userId);

    public async Task<UploadedStatement> CreateAsync(UploadedStatement statement)
    {
        _db.UploadedStatements.Add(statement);
        await _db.SaveChangesAsync();
        return statement;
    }

    public async Task UpdateAsync(UploadedStatement statement)
    {
        _db.UploadedStatements.Update(statement);
        await _db.SaveChangesAsync();
    }
}
