using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StudyMate.Data;
using StudyMate.Models;
using StudyMate.Services.Interfaces;

namespace StudyMate.Controllers;

[Authorize]
public class MatchingController : Controller
{
    private readonly ApplicationDbContext _db;
    private readonly UserManager<ApplicationUser> _users;
    private readonly IMatchingService _matching;

    public MatchingController(
        ApplicationDbContext db,
        UserManager<ApplicationUser> users,
        IMatchingService matching)
    {
        _db = db;
        _users = users;
        _matching = matching;
    }

    [Authorize(Roles = "Tutor")]
    [HttpGet]
    public async Task<IActionResult> RecommendedJobs()
    {
        var me = _users.GetUserId(User)!;
        var profile = await _db.TutorProfiles.AsNoTracking().FirstOrDefaultAsync(p => p.UserId == me);
        if (profile == null)
        {
            TempData["Error"] = "Cần tạo hồ sơ gia sư.";
            return RedirectToAction("MyProfile", "TutorProfile");
        }

        var recs = await _matching.RecommendJobsForTutorAsync(profile.Id);
        var ids = recs.Select(r => r.JobId).ToList();
        var jobs = await _db.JobPostings.AsNoTracking()
            .Include(j => j.Subject)
            .Where(j => ids.Contains(j.Id))
            .ToListAsync();

        var model = recs
            .Select(r => (Job: jobs.FirstOrDefault(j => j.Id == r.JobId), r.Score))
            .Where(x => x.Job != null)
            .ToList();
        return View(model);
    }

    [Authorize(Roles = "Student")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Rematch(int jobId)
    {
        var me = _users.GetUserId(User)!;
        var job = await _db.JobPostings.FirstOrDefaultAsync(j => j.Id == jobId && j.StudentId == me);
        if (job == null) return NotFound();
        await _matching.MatchJobToTutorsAsync(jobId);
        TempData["Success"] = "Đã chạy lại AI matching.";
        return RedirectToAction("Details", "JobPosting", new { id = jobId });
    }
}
