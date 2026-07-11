namespace StudyMate.Models;

/// <summary>Tutor apply vào Job (table Applications).</summary>
public class JobApplication
{
    public int Id { get; set; }
    public int JobPostingId { get; set; }
    public string TutorId { get; set; } = string.Empty;
    public string? CoverNote { get; set; }
    public decimal? ProposedRate { get; set; }
    /// <summary>Pending / Accepted / Rejected / Cancelled</summary>
    public string Status { get; set; } = "Pending";
    public DateTime AppliedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    public JobPosting? JobPosting { get; set; }
    public ApplicationUser? Tutor { get; set; }
}
