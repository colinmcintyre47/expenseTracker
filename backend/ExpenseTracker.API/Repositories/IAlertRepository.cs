using ExpenseTracker.API.Models;

namespace ExpenseTracker.API.Repositories;

public interface IAlertRepository
{
    Task<List<Alert>> GetForUserAsync(int userId, bool unreadOnly = false);
    Task<int> GetUnreadCountAsync(int userId);
    Task<Alert> CreateAsync(Alert alert);
    Task MarkReadAsync(int id, int userId);
    Task MarkAllReadAsync(int userId);
}
