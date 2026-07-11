namespace StudyMate.Models;

/// <summary>
/// Bài học mẫu (demo) trên hồ sơ gia sư.
/// </summary>
public class DemoLesson
{
    public int Id { get; set; }

    public int TutorProfileId { get; set; }

    public string Title { get; set; } = string.Empty;

    public string? Description { get; set; }

    /// <summary>URL video (YouTube hoặc path local).</summary>
    public string? VideoUrl { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public TutorProfile? TutorProfile { get; set; }
}
