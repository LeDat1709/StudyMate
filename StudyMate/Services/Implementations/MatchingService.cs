using Microsoft.EntityFrameworkCore;
using StudyMate.Data;
using StudyMate.Models;
using StudyMate.Services.Interfaces;

namespace StudyMate.Services.Implementations;

/// <summary>M4 stub scoring (subject + keyword). Swap to Python FastAPI later.</summary>
public class MatchingService : IMatchingService
{
    private readonly ApplicationDbContext _db;
    private readonly ILogger<MatchingService> _logger;

    public MatchingService(ApplicationDbContext db, ILogger<MatchingService> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task MatchJobToTutorsAsync(int jobPostingId)
    {
        var job = await _db.JobPostings.Include(j => j.Subject)
            .FirstOrDefaultAsync(j => j.Id == jobPostingId);
        if (job == null) return;

        var tutors = await _db.TutorProfiles.AsNoTracking()
            .Include(t => t.TutorSubjects)
            .Where(t => t.IsVerified && t.IsAvailable)
            .ToListAsync();

        var jobText = $"{job.Title} {job.Description} {job.DesiredLevel} {job.Subject?.Name}".ToLowerInvariant();
        var tokens = jobText.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Where(w => w.Length > 2).Distinct().Take(20).ToList();

        var scored = tutors.Select(t =>
            {
                double score = 0.1;
                if (t.TutorSubjects.Any(ts => ts.SubjectId == job.SubjectId)) score += 0.5;
                var tutorText = $"{t.Headline} {t.Bio} {t.EducationLevel}".ToLowerInvariant();
                score += Math.Min(0.4, tokens.Count(tok => tutorText.Contains(tok)) * 0.05);
                if (job.BudgetMax is not null && t.HourlyRate is not null && t.HourlyRate <= job.BudgetMax)
                    score += 0.1;
                return (t.Id, Score: (decimal)Math.Min(0.99, score));
            })
            .OrderByDescending(x => x.Score)
            .Take(10)
            .Select((x, i) => new MatchingResult
            {
                JobPostingId = job.Id,
                StudentId = job.StudentId,
                TutorProfileId = x.Id,
                SimilarityScore = x.Score,
                Rank = i + 1,
                ModelVersion = "stub-v1",
                CreatedAt = DateTime.UtcNow
            })
            .ToList();

        _db.MatchingResults.RemoveRange(_db.MatchingResults.Where(m => m.JobPostingId == jobPostingId));
        _db.MatchingResults.AddRange(scored);
        await _db.SaveChangesAsync();
        _logger.LogInformation("Matched {N} tutors for job {Id}", scored.Count, jobPostingId);
    }

    public async Task<IReadOnlyList<(int JobId, decimal Score)>> RecommendJobsForTutorAsync(int tutorProfileId, int top = 10)
    {
        var tutor = await _db.TutorProfiles.AsNoTracking()
            .Include(t => t.TutorSubjects)
            .FirstOrDefaultAsync(t => t.Id == tutorProfileId);
        if (tutor == null) return Array.Empty<(int, decimal)>();

        var subjectIds = tutor.TutorSubjects.Select(ts => ts.SubjectId).ToHashSet();
        var jobs = await _db.JobPostings.AsNoTracking().Where(j => j.Status == "Open").ToListAsync();
        return jobs.Select(j =>
            {
                decimal score = 0.1m;
                if (subjectIds.Contains(j.SubjectId)) score += 0.6m;
                return (j.Id, score);
            })
            .OrderByDescending(x => x.score)
            .Take(top)
            .ToList();
    }
}
