using Microsoft.EntityFrameworkCore;
using StudyMate.Data;
using StudyMate.Models;
using StudyMate.Services.Interfaces;

namespace StudyMate.Services.Implementations;
public class NotificationService : INotificationService
{
    private readonly ApplicationDbContext _db;
    public NotificationService(ApplicationDbContext db) => _db = db;
    public async Task NotifyAsync(string userId, string title, string? body, string? type = null, int? referenceId = null)
    {
        _db.Notifications.Add(new Notification {
            UserId = userId, Title = title, Body = body, Type = type,
            ReferenceId = referenceId, IsRead = false, CreatedAt = DateTime.UtcNow
        });
        await _db.SaveChangesAsync();
    }
    public Task<int> CountUnreadAsync(string userId) =>
        _db.Notifications.CountAsync(n => n.UserId == userId && !n.IsRead);
}
