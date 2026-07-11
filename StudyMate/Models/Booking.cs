namespace StudyMate.Models;

/// <summary>Lịch học (M7).</summary>
public class Booking
{
    public int Id { get; set; }
    public int ApplicationId { get; set; }
    public string StudentId { get; set; } = string.Empty;
    public string TutorId { get; set; } = string.Empty;
    public DateTime ScheduledStart { get; set; }
    public DateTime ScheduledEnd { get; set; }
    public string? MeetingUrl { get; set; }
    /// <summary>Pending / Confirmed / Cancelled / Completed</summary>
    public string Status { get; set; } = "Pending";
    public DateTime? CheckInAt { get; set; }
    public DateTime? CheckOutAt { get; set; }
    public string? Note { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public JobApplication? Application { get; set; }
    public ApplicationUser? Student { get; set; }
    public ApplicationUser? Tutor { get; set; }
}
