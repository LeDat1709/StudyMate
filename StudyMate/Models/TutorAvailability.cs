namespace StudyMate.Models;

/// <summary>
/// Khung giờ rảnh trong tuần của gia sư.
/// DayOfWeek: 0 = Sunday … 6 = Saturday (chuẩn <see cref="System.DayOfWeek"/>).
/// </summary>
public class TutorAvailability
{
    public int Id { get; set; }

    public int TutorProfileId { get; set; }

    /// <summary>0=Sunday … 6=Saturday.</summary>
    public int DayOfWeek { get; set; }

    public TimeOnly StartTime { get; set; }

    public TimeOnly EndTime { get; set; }

    public TutorProfile? TutorProfile { get; set; }
}
