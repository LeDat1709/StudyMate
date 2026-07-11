namespace StudyMate.Services.Interfaces;
public interface INotificationService
{
    Task NotifyAsync(string userId, string title, string? body, string? type = null, int? referenceId = null);
    Task<int> CountUnreadAsync(string userId);
}
