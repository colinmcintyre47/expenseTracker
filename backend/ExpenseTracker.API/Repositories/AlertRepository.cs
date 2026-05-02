using ExpenseTracker.API.Data;
using ExpenseTracker.API.Models;
using Microsoft.EntityFrameworkCore;

namespace ExpenseTracker.API.Repositories;

public class AlertRepository : IAlertRepository
{
    private readonly AppDbContext _db;
    public AlertRepository(AppDbContext db) => _db = db;

    public async Task<List<Alert>> GetForUserAsync(int userId, bool unreadOnly = false)
    {
        var query = _db.Alerts.Where(a => a.UserId == userId);
        if (unreadOnly)
            query = query.Where(a => !a.IsRead);
        return await query.OrderByDescending(a => a.CreatedAt).Take(100).ToListAsync();
    }

    public async Task<int> GetUnreadCountAsync(int userId)
        => await _db.Alerts.CountAsync(a => a.UserId == userId && !a.IsRead);

    public async Task<Alert> CreateAsync(Alert alert)
    {
        _db.Alerts.Add(alert);
        await _db.SaveChangesAsync();
        return alert;
    }

    public async Task MarkReadAsync(int id, int userId)
    {
        var alert = await _db.Alerts.FirstOrDefaultAsync(a => a.Id == id && a.UserId == userId);
        if (alert == null) return;
        alert.IsRead = true;
        await _db.SaveChangesAsync();
    }

    public async Task MarkAllReadAsync(int userId)
    {
        await _db.Alerts
            .Where(a => a.UserId == userId && !a.IsRead)
            .ExecuteUpdateAsync(s => s.SetProperty(a => a.IsRead, true));
    }
}
