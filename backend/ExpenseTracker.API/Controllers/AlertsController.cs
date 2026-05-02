using ExpenseTracker.API.DTOs.Alerts;
using ExpenseTracker.API.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace ExpenseTracker.API.Controllers;

/// <summary>
/// Alert management endpoints.
/// Alerts are created by the system (budget threshold crossed, anomaly detected)
/// and dismissed (marked as read) by the user.
/// </summary>
[Route("api/alerts")]
public class AlertsController : BaseAuthController
{
    private readonly IAlertRepository _alertRepo;

    public AlertsController(IAlertRepository alertRepo)
    {
        _alertRepo = alertRepo;
    }

    /// <summary>Returns all alerts for the current user, newest first.</summary>
    [HttpGet]
    public async Task<IActionResult> GetAlerts([FromQuery] bool unreadOnly = false)
    {
        var alerts = await _alertRepo.GetForUserAsync(UserId, unreadOnly);
        var dtos = alerts.Select(a => new AlertDto
        {
            Id = a.Id,
            TransactionId = a.TransactionId,
            Type = a.Type,
            Message = a.Message,
            IsRead = a.IsRead,
            CreatedAt = a.CreatedAt
        }).ToList();
        return Ok(dtos);
    }

    /// <summary>Mark a single alert as read.</summary>
    [HttpPut("{id}/read")]
    public async Task<IActionResult> MarkRead(int id)
    {
        await _alertRepo.MarkReadAsync(id, UserId);
        return NoContent();
    }

    /// <summary>Mark all alerts as read (dismiss all).</summary>
    [HttpPut("read-all")]
    public async Task<IActionResult> MarkAllRead()
    {
        await _alertRepo.MarkAllReadAsync(UserId);
        return NoContent();
    }
}
