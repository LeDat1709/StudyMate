namespace StudyMate.Models;

/// <summary>Kết quả AI matching job ↔ tutor (M4).</summary>
public class MatchingResult
{
    public int Id { get; set; }
    public int? JobPostingId { get; set; }
    public string StudentId { get; set; } = string.Empty;
    public int TutorProfileId { get; set; }
    public decimal SimilarityScore { get; set; }
    public int Rank { get; set; }
    public string? ModelVersion { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public JobPosting? JobPosting { get; set; }
    public ApplicationUser? Student { get; set; }
    public TutorProfile? TutorProfile { get; set; }
}
