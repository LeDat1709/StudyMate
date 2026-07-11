namespace StudyMate.Services.Interfaces;

public interface IMatchingService
{
    Task MatchJobToTutorsAsync(int jobPostingId);
    Task<IReadOnlyList<(int JobId, decimal Score)>> RecommendJobsForTutorAsync(int tutorProfileId, int top = 10);
}
