namespace StudyMate.Models;

/// <summary>
/// Môn học có sẵn trên hệ thống (Toán, IELTS, …). Seed ở M2-T1.
/// </summary>
public class Subject
{
    public int Id { get; set; }

    /// <summary>Tên môn (VD: IELTS, Lập trình C#).</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Nhóm môn: THPT / Ngoại ngữ / Công nghệ …</summary>
    public string? Category { get; set; }

    public ICollection<TutorSubject> TutorSubjects { get; set; } = new List<TutorSubject>();
}
