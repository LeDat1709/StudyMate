namespace StudyMate.Models;
public class Review
{
    public int Id { get; set; }
    public int BookingId { get; set; }
    public string ReviewerId { get; set; } = string.Empty;
    public int TutorProfileId { get; set; }
    public byte Rating { get; set; }
    public string? Comment { get; set; }
    public string? TutorReply { get; set; }
    public bool IsSpam { get; set; }
    public string? AiSpamNote { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public Booking? Booking { get; set; }
    public ApplicationUser? Reviewer { get; set; }
    public TutorProfile? TutorProfile { get; set; }
}
