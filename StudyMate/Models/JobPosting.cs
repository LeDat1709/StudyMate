namespace StudyMate.Models;

/// <summary>
/// Yêu cầu thuê gia sư (M3). Schema: JobPostings.
/// </summary>
public class JobPosting
{
    public int Id { get; set; }

    /// <summary>UserId học viên đăng job.</summary>
    public string StudentId { get; set; } = string.Empty;

    public string Title { get; set; } = string.Empty;

    public string? Description { get; set; }

    public int SubjectId { get; set; }

    /// <summary>VD: IELTS 7.5+, lớp 12.</summary>
    public string? DesiredLevel { get; set; }

    /// <summary>Online / Offline / Both</summary>
    public string? TeachingMode { get; set; }

    public string? Address { get; set; }

    public decimal? BudgetMin { get; set; }

    public decimal? BudgetMax { get; set; }

    public int? SessionsPerWeek { get; set; }

    /// <summary>Phút mỗi buổi.</summary>
    public int? SessionDuration { get; set; }

    public DateTime? Deadline { get; set; }

    /// <summary>Open / Closed / Expired</summary>
    public string Status { get; set; } = "Open";

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }

    public ApplicationUser? Student { get; set; }

    public Subject? Subject { get; set; }
}
