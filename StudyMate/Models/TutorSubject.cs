namespace StudyMate.Models;

/// <summary>
/// Môn dạy của gia sư (many-to-many TutorProfile ↔ Subject).
/// Composite key: TutorProfileId + SubjectId.
/// </summary>
public class TutorSubject
{
    public int TutorProfileId { get; set; }

    public int SubjectId { get; set; }

    public TutorProfile? TutorProfile { get; set; }

    public Subject? Subject { get; set; }
}
