using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StudyMate.Data;
using StudyMate.Models;
using StudyMate.Services.Interfaces;

namespace StudyMate.Controllers;

[Authorize]
public class ApplicationController : Controller
{
    private readonly ApplicationDbContext _db;
    private readonly UserManager<ApplicationUser> _users;
    private readonly INotificationService _notify;

    public ApplicationController(ApplicationDbContext db, UserManager<ApplicationUser> users, INotificationService notify)
    {
        _db = db;
        _users = users;
        _notify = notify;
    }

    [Authorize(Roles = "Tutor")]
    [HttpGet]
    public async Task<IActionResult> Apply(int jobId)
    {
        var job = await _db.JobPostings.AsNoTracking()
            .FirstOrDefaultAsync(j => j.Id == jobId && j.Status == "Open");
        if (job == null) return NotFound();

        var user = await _users.GetUserAsync(User);
        if (user == null) return Challenge();
        if (!await _db.TutorProfiles.AnyAsync(p => p.UserId == user.Id && p.IsVerified))
        {
            TempData["Error"] = "Cáº§n há»“ sÆ¡ gia sÆ° Ä‘Ă£ duyá»‡t (IsVerified).";
            return RedirectToAction("Details", "JobPosting", new { id = jobId });
        }
        if (await _db.Applications.AnyAsync(a => a.JobPostingId == jobId && a.TutorId == user.Id))
        {
            TempData["Error"] = "Báº¡n Ä‘Ă£ apply job nĂ y.";
            return RedirectToAction("Details", "JobPosting", new { id = jobId });
        }

        ViewData["JobTitle"] = job.Title;
        return View(new ApplyFormVm { JobPostingId = jobId });
    }

    [Authorize(Roles = "Tutor")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Apply(ApplyFormVm model)
    {
        var user = await _users.GetUserAsync(User);
        if (user == null) return Challenge();

        var job = await _db.JobPostings.FirstOrDefaultAsync(j => j.Id == model.JobPostingId && j.Status == "Open");
        if (job == null) return NotFound();

        if (!await _db.TutorProfiles.AnyAsync(p => p.UserId == user.Id && p.IsVerified))
        {
            TempData["Error"] = "Cáº§n há»“ sÆ¡ gia sÆ° Ä‘Ă£ duyá»‡t.";
            return RedirectToAction("Details", "JobPosting", new { id = model.JobPostingId });
        }
        if (await _db.Applications.AnyAsync(a => a.JobPostingId == model.JobPostingId && a.TutorId == user.Id))
        {
            TempData["Error"] = "Báº¡n Ä‘Ă£ apply job nĂ y.";
            return RedirectToAction("Details", "JobPosting", new { id = model.JobPostingId });
        }

        if (model.CoverNote?.Length > 500)
            ModelState.AddModelError(nameof(model.CoverNote), "Tá»‘i Ä‘a 500 kĂ½ tá»±.");
        if (!ModelState.IsValid)
        {
            ViewData["JobTitle"] = job.Title;
            return View(model);
        }

        _db.Applications.Add(new JobApplication
        {
            JobPostingId = model.JobPostingId,
            TutorId = user.Id,
            CoverNote = model.CoverNote?.Trim(),
            ProposedRate = model.ProposedRate,
            Status = "Pending",
            AppliedAt = DateTime.UtcNow
        });
        await _db.SaveChangesAsync();
        TempData["Success"] = "Apply thĂ nh cĂ´ng.";
        return RedirectToAction("Details", "JobPosting", new { id = model.JobPostingId });
    }

    [Authorize(Roles = "Student")]
    [HttpGet]
    public async Task<IActionResult> ForJob(int jobId)
    {
        var me = _users.GetUserId(User)!;
        var job = await _db.JobPostings.AsNoTracking()
            .FirstOrDefaultAsync(j => j.Id == jobId && j.StudentId == me);
        if (job == null) return NotFound();

        var list = await _db.Applications.AsNoTracking()
            .Include(a => a.Tutor)
            .Where(a => a.JobPostingId == jobId)
            .OrderByDescending(a => a.AppliedAt)
            .ToListAsync();
        ViewData["JobTitle"] = job.Title;
        ViewData["JobId"] = jobId;
        return View(list);
    }

    [Authorize(Roles = "Student")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Accept(int id)
    {
        var app = await LoadOwnedAppAsync(id);
        if (app == null) return NotFound();
        if (app.Status != "Pending") return BadRequest();
        app.Status = "Accepted";
        app.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        await _notify.NotifyAsync(app.TutorId, "Application được chấp nhận", $"JobId={app.JobPostingId}", "ApplyAccepted", app.Id);
        return RedirectToAction(nameof(ForJob), new { jobId = app.JobPostingId });
    }

    [Authorize(Roles = "Student")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Reject(int id)
    {
        var app = await LoadOwnedAppAsync(id);
        if (app == null) return NotFound();
        if (app.Status != "Pending") return BadRequest();
        app.Status = "Rejected";
        app.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return RedirectToAction(nameof(ForJob), new { jobId = app.JobPostingId });
    }

    [Authorize(Roles = "Tutor")]
    [HttpGet]
    public async Task<IActionResult> MyApplications()
    {
        var me = _users.GetUserId(User)!;
        var list = await _db.Applications.AsNoTracking()
            .Include(a => a.JobPosting)
            .Where(a => a.TutorId == me)
            .OrderByDescending(a => a.AppliedAt)
            .ToListAsync();
        return View(list);
    }

    [Authorize(Roles = "Tutor")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Cancel(int id)
    {
        var me = _users.GetUserId(User)!;
        var app = await _db.Applications.FirstOrDefaultAsync(a => a.Id == id && a.TutorId == me);
        if (app == null) return NotFound();
        if (app.Status != "Pending") return BadRequest();
        app.Status = "Cancelled";
        app.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return RedirectToAction(nameof(MyApplications));
    }

    private async Task<JobApplication?> LoadOwnedAppAsync(int id)
    {
        var me = _users.GetUserId(User)!;
        var app = await _db.Applications.Include(a => a.JobPosting).FirstOrDefaultAsync(a => a.Id == id);
        if (app?.JobPosting == null || app.JobPosting.StudentId != me) return null;
        return app;
    }
}

public class ApplyFormVm
{
    public int JobPostingId { get; set; }

    [StringLength(500)]
    [Display(Name = "ThÆ° giá»›i thiá»‡u")]
    public string? CoverNote { get; set; }

    [Display(Name = "Há»c phĂ­ Ä‘á» xuáº¥t")]
    public decimal? ProposedRate { get; set; }
}

