using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StudyMate.Data;
using StudyMate.Models;
using StudyMate.Services.Interfaces;
using StudyMate.ViewModels.JobPosting;

namespace StudyMate.Controllers;

/// <summary>M3 — Job Posting (no Apply/Matching).</summary>
public class JobPostingController : Controller
{
    private const int MaxOpenJobs = 5;
    private const int PageSize = 10;

    private readonly ApplicationDbContext _db;
    private readonly UserManager<ApplicationUser> _users;
    private readonly IMatchingService _matching;
    private readonly ILogger<JobPostingController> _logger;

    public static readonly string[] TeachingModes = ["Online", "Offline", "Both"];

    public JobPostingController(
        ApplicationDbContext db,
        UserManager<ApplicationUser> users,
        IMatchingService matching,
        ILogger<JobPostingController> logger)
    {
        _db = db;
        _users = users;
        _matching = matching;
        _logger = logger;
    }

    // ── M3-T8 expire helper ──────────────────────────────────────────────────

    private async Task ExpireOverdueJobsAsync()
    {
        var now = DateTime.UtcNow;
        var list = await _db.JobPostings
            .Where(j => j.Status == "Open" && j.Deadline != null && j.Deadline < now)
            .ToListAsync();
        if (list.Count == 0) return;
        foreach (var j in list)
        {
            j.Status = "Expired";
            j.UpdatedAt = now;
        }
        await _db.SaveChangesAsync();
        _logger.LogInformation("Expired {Count} job postings", list.Count);
    }

    // ── M3-T2 Create ─────────────────────────────────────────────────────────

    [Authorize(Roles = "Student")]
    [HttpGet]
    public async Task<IActionResult> Create()
    {
        return View(await BuildFormAsync(new JobPostingFormViewModel { TeachingMode = "Online" }));
    }

    [Authorize(Roles = "Student")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(JobPostingFormViewModel model)
    {
        var user = await _users.GetUserAsync(User);
        if (user == null) return Challenge();

        ValidateBusiness(model);

        var openCount = await _db.JobPostings.CountAsync(j => j.StudentId == user.Id && j.Status == "Open");
        if (openCount >= MaxOpenJobs)
            ModelState.AddModelError(string.Empty, $"Bạn chỉ được có tối đa {MaxOpenJobs} job đang Open.");

        if (!ModelState.IsValid)
            return View(await BuildFormAsync(model));

        var entity = new JobPosting
        {
            StudentId = user.Id,
            Title = model.Title.Trim(),
            Description = model.Description.Trim(),
            SubjectId = model.SubjectId,
            DesiredLevel = NullIfEmpty(model.DesiredLevel),
            TeachingMode = model.TeachingMode,
            Address = model.TeachingMode == "Online" ? null : NullIfEmpty(model.Address),
            BudgetMin = model.BudgetMin,
            BudgetMax = model.BudgetMax,
            SessionsPerWeek = model.SessionsPerWeek,
            SessionDuration = model.SessionDuration,
            Deadline = model.Deadline,
            Status = "Open",
            CreatedAt = DateTime.UtcNow
        };
        _db.JobPostings.Add(entity);
        await _db.SaveChangesAsync();
        _logger.LogInformation("JobPosting created Id={Id} by {User}", entity.Id, user.Email);

        // M4: AI match tutors for this job (stub)
        await _matching.MatchJobToTutorsAsync(entity.Id);

        TempData["Success"] = "Đăng yêu cầu thành công.";
        return RedirectToAction(nameof(Details), new { id = entity.Id });
    }

    // ── M3-T3 / M3-T6 Index list + search ────────────────────────────────────

    [AllowAnonymous]
    [HttpGet]
    public async Task<IActionResult> Index(
        string? q,
        int? subjectId,
        string? teachingMode,
        decimal? budgetMin,
        decimal? budgetMax,
        string? address,
        string sort = "Newest",
        int page = 1)
    {
        await ExpireOverdueJobsAsync();
        if (page < 1) page = 1;

        var query = _db.JobPostings.AsNoTracking()
            .Include(j => j.Subject)
            .Include(j => j.Student)
            .Where(j => j.Status == "Open");

        if (!string.IsNullOrWhiteSpace(q))
        {
            var term = q.Trim();
            query = query.Where(j =>
                j.Title.Contains(term) ||
                (j.Description != null && j.Description.Contains(term)));
        }
        if (subjectId is > 0)
            query = query.Where(j => j.SubjectId == subjectId.Value);
        if (!string.IsNullOrWhiteSpace(teachingMode))
            query = query.Where(j => j.TeachingMode == teachingMode || j.TeachingMode == "Both");
        if (budgetMin is not null)
            query = query.Where(j => j.BudgetMax == null || j.BudgetMax >= budgetMin);
        if (budgetMax is not null)
            query = query.Where(j => j.BudgetMin == null || j.BudgetMin <= budgetMax);
        if (!string.IsNullOrWhiteSpace(address))
        {
            var a = address.Trim();
            query = query.Where(j => j.Address != null && j.Address.Contains(a));
        }

        query = sort switch
        {
            "BudgetDesc" => query.OrderByDescending(j => j.BudgetMax ?? j.BudgetMin ?? 0),
            "Deadline" => query.OrderBy(j => j.Deadline ?? DateTime.MaxValue),
            _ => query.OrderByDescending(j => j.CreatedAt)
        };

        var total = await query.CountAsync();
        var items = await query
            .Skip((page - 1) * PageSize)
            .Take(PageSize)
            .Select(j => new JobCardViewModel
            {
                Id = j.Id,
                Title = j.Title,
                DescriptionSnippet = j.Description,
                SubjectName = j.Subject != null ? j.Subject.Name : null,
                TeachingMode = j.TeachingMode,
                BudgetMin = j.BudgetMin,
                BudgetMax = j.BudgetMax,
                Deadline = j.Deadline,
                Status = j.Status,
                CreatedAt = j.CreatedAt,
                StudentName = j.Student != null ? j.Student.FullName : null
            })
            .ToListAsync();

        foreach (var item in items)
        {
            if (string.IsNullOrWhiteSpace(item.DescriptionSnippet)) continue;
            var text = item.DescriptionSnippet.Trim();
            item.DescriptionSnippet = text.Length <= 180 ? text : text[..180].TrimEnd() + "…";
        }

        return View(new JobListViewModel
        {
            Items = items,
            Page = page,
            PageSize = PageSize,
            TotalCount = total,
            Keyword = q,
            SubjectId = subjectId,
            TeachingMode = teachingMode,
            BudgetMin = budgetMin,
            BudgetMax = budgetMax,
            Address = address,
            Sort = sort,
            Subjects = await _db.Subjects.AsNoTracking().OrderBy(s => s.Name).ToListAsync()
        });
    }

    // ── M3-T4 Details ────────────────────────────────────────────────────────

    [AllowAnonymous]
    [HttpGet]
    public async Task<IActionResult> Details(int id)
    {
        await ExpireOverdueJobsAsync();
        var j = await _db.JobPostings.AsNoTracking()
            .Include(x => x.Subject)
            .Include(x => x.Student)
            .FirstOrDefaultAsync(x => x.Id == id);
        if (j == null) return NotFound();

        var userId = _users.GetUserId(User);
        var already = userId != null && await _db.Applications.AnyAsync(a => a.JobPostingId == id && a.TutorId == userId);
        var matched = await _db.MatchingResults.AsNoTracking()
            .Include(m => m.TutorProfile)!.ThenInclude(t => t!.User)
            .Where(m => m.JobPostingId == id)
            .OrderBy(m => m.Rank)
            .Take(10)
            .Select(m => new MatchedTutorItem
            {
                TutorProfileId = m.TutorProfileId,
                FullName = m.TutorProfile != null && m.TutorProfile.User != null
                    ? m.TutorProfile.User.FullName : null,
                Headline = m.TutorProfile != null ? m.TutorProfile.Headline : null,
                Score = m.SimilarityScore,
                Rank = m.Rank
            })
            .ToListAsync();

        return View(new JobDetailViewModel
        {
            Id = j.Id,
            Title = j.Title,
            Description = j.Description,
            SubjectName = j.Subject?.Name,
            DesiredLevel = j.DesiredLevel,
            TeachingMode = j.TeachingMode,
            Address = j.Address,
            BudgetMin = j.BudgetMin,
            BudgetMax = j.BudgetMax,
            SessionsPerWeek = j.SessionsPerWeek,
            SessionDuration = j.SessionDuration,
            Deadline = j.Deadline,
            Status = j.Status,
            CreatedAt = j.CreatedAt,
            StudentId = j.StudentId,
            StudentName = j.Student?.FullName,
            StudentAvatar = j.Student?.AvatarUrl,
            IsOwner = userId == j.StudentId,
            CanApply = User.IsInRole("Tutor") && j.Status == "Open" && !already,
            AlreadyApplied = already,
            MatchedTutors = matched
        });
    }

    // ── M3-T5 Edit / Close / Delete ──────────────────────────────────────────

    [Authorize(Roles = "Student")]
    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var user = await _users.GetUserAsync(User);
        if (user == null) return Challenge();
        var j = await _db.JobPostings.FirstOrDefaultAsync(x => x.Id == id && x.StudentId == user.Id);
        if (j == null) return NotFound();
        if (j.Status != "Open")
        {
            TempData["Error"] = "Chỉ sửa được job đang Open.";
            return RedirectToAction(nameof(Details), new { id });
        }

        return View(await BuildFormAsync(new JobPostingFormViewModel
        {
            Id = j.Id,
            Title = j.Title,
            Description = j.Description ?? "",
            SubjectId = j.SubjectId,
            DesiredLevel = j.DesiredLevel,
            TeachingMode = j.TeachingMode ?? "Online",
            Address = j.Address,
            BudgetMin = j.BudgetMin,
            BudgetMax = j.BudgetMax,
            SessionsPerWeek = j.SessionsPerWeek,
            SessionDuration = j.SessionDuration,
            Deadline = j.Deadline
        }));
    }

    [Authorize(Roles = "Student")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, JobPostingFormViewModel model)
    {
        var user = await _users.GetUserAsync(User);
        if (user == null) return Challenge();
        var j = await _db.JobPostings.FirstOrDefaultAsync(x => x.Id == id && x.StudentId == user.Id);
        if (j == null) return NotFound();
        if (j.Status != "Open")
        {
            TempData["Error"] = "Chỉ sửa được job đang Open.";
            return RedirectToAction(nameof(Details), new { id });
        }

        ValidateBusiness(model);
        if (!ModelState.IsValid)
        {
            model.Id = id;
            return View(await BuildFormAsync(model));
        }

        j.Title = model.Title.Trim();
        j.Description = model.Description.Trim();
        j.SubjectId = model.SubjectId;
        j.DesiredLevel = NullIfEmpty(model.DesiredLevel);
        j.TeachingMode = model.TeachingMode;
        j.Address = model.TeachingMode == "Online" ? null : NullIfEmpty(model.Address);
        j.BudgetMin = model.BudgetMin;
        j.BudgetMax = model.BudgetMax;
        j.SessionsPerWeek = model.SessionsPerWeek;
        j.SessionDuration = model.SessionDuration;
        j.Deadline = model.Deadline;
        j.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        TempData["Success"] = "Cập nhật job thành công.";
        return RedirectToAction(nameof(Details), new { id });
    }

    [Authorize(Roles = "Student")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Close(int id)
    {
        var user = await _users.GetUserAsync(User);
        if (user == null) return Challenge();
        var j = await _db.JobPostings.FirstOrDefaultAsync(x => x.Id == id && x.StudentId == user.Id);
        if (j == null) return NotFound();
        j.Status = "Closed";
        j.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        TempData["Success"] = "Đã đóng job.";
        return RedirectToAction(nameof(MyJobs));
    }

    [Authorize(Roles = "Student")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var user = await _users.GetUserAsync(User);
        if (user == null) return Challenge();
        var j = await _db.JobPostings.FirstOrDefaultAsync(x => x.Id == id && x.StudentId == user.Id);
        if (j == null) return NotFound();

        if (await _db.Applications.AnyAsync(a => a.JobPostingId == id))
        {
            TempData["Error"] = "Job đã có application — chỉ được Đóng, không xóa.";
            return RedirectToAction(nameof(MyJobs));
        }

        _db.JobPostings.Remove(j);
        await _db.SaveChangesAsync();
        TempData["Success"] = "Đã xóa job.";
        return RedirectToAction(nameof(MyJobs));
    }

    // ── M3-T7 My Jobs ────────────────────────────────────────────────────────

    [Authorize(Roles = "Student")]
    [HttpGet]
    public async Task<IActionResult> MyJobs(string? status)
    {
        await ExpireOverdueJobsAsync();
        var user = await _users.GetUserAsync(User);
        if (user == null) return Challenge();

        var query = _db.JobPostings.AsNoTracking()
            .Include(j => j.Subject)
            .Where(j => j.StudentId == user.Id);

        if (!string.IsNullOrWhiteSpace(status) && status != "All")
            query = query.Where(j => j.Status == status);

        var items = await query.OrderByDescending(j => j.CreatedAt)
            .Select(j => new JobCardViewModel
            {
                Id = j.Id,
                Title = j.Title,
                SubjectName = j.Subject != null ? j.Subject.Name : null,
                TeachingMode = j.TeachingMode,
                BudgetMin = j.BudgetMin,
                BudgetMax = j.BudgetMax,
                Deadline = j.Deadline,
                Status = j.Status,
                CreatedAt = j.CreatedAt
            })
            .ToListAsync();

        ViewData["Status"] = status ?? "All";
        return View(items);
    }

    // ── helpers ──────────────────────────────────────────────────────────────

    private void ValidateBusiness(JobPostingFormViewModel model)
    {
        if (!TeachingModes.Contains(model.TeachingMode))
            ModelState.AddModelError(nameof(model.TeachingMode), "Hình thức không hợp lệ.");

        if (model.TeachingMode is "Offline" or "Both" && string.IsNullOrWhiteSpace(model.Address))
            ModelState.AddModelError(nameof(model.Address), "Offline/Both bắt buộc địa điểm.");

        if (model.Deadline is not null && model.Deadline.Value.Date < DateTime.UtcNow.Date)
            ModelState.AddModelError(nameof(model.Deadline), "Deadline phải là ngày trong tương lai.");

        if (model.BudgetMin is not null && model.BudgetMax is not null && model.BudgetMin > model.BudgetMax)
            ModelState.AddModelError(nameof(model.BudgetMax), "Budget max phải ≥ min.");

        if (!_db.Subjects.Any(s => s.Id == model.SubjectId))
            ModelState.AddModelError(nameof(model.SubjectId), "Môn học không hợp lệ.");
    }

    private async Task<JobPostingFormViewModel> BuildFormAsync(JobPostingFormViewModel model)
    {
        model.Subjects = await _db.Subjects.AsNoTracking().OrderBy(s => s.Name).ToListAsync();
        return model;
    }

    private static string? NullIfEmpty(string? s) =>
        string.IsNullOrWhiteSpace(s) ? null : s.Trim();
}
