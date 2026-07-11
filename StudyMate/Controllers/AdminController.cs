using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StudyMate.Data;
using StudyMate.Models;

namespace StudyMate.Controllers;

[Authorize(Roles = "Admin")]
public class AdminController : Controller
{
    private readonly ApplicationDbContext _db;
    private readonly UserManager<ApplicationUser> _users;

    public AdminController(ApplicationDbContext db, UserManager<ApplicationUser> users)
    {
        _db = db;
        _users = users;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        ViewData["Users"] = await _db.Users.CountAsync();
        ViewData["Jobs"] = await _db.JobPostings.CountAsync();
        ViewData["Tutors"] = await _db.TutorProfiles.CountAsync();
        ViewData["PendingProfiles"] = await _db.TutorProfiles.CountAsync(p => !p.IsVerified);
        ViewData["PendingReports"] = await _db.Reports.CountAsync(r => r.Status == "Pending");
        return View();
    }

    [HttpGet]
    public async Task<IActionResult> Users()
    {
        var users = await _db.Users.AsNoTracking().OrderByDescending(u => u.CreatedAt).Take(200).ToListAsync();
        return View(users);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleUserActive(string id)
    {
        var u = await _users.FindByIdAsync(id);
        if (u == null) return NotFound();
        u.IsActive = !u.IsActive;
        await _users.UpdateAsync(u);
        return RedirectToAction(nameof(Users));
    }

    [HttpGet]
    public async Task<IActionResult> TutorProfiles()
    {
        var list = await _db.TutorProfiles.AsNoTracking()
            .Include(p => p.User)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync();
        return View(list);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> VerifyTutor(int id, bool verify)
    {
        var p = await _db.TutorProfiles.FindAsync(id);
        if (p == null) return NotFound();
        p.IsVerified = verify;
        p.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return RedirectToAction(nameof(TutorProfiles));
    }

    [HttpGet]
    public async Task<IActionResult> Reports()
    {
        var list = await _db.Reports.AsNoTracking()
            .Include(r => r.Reporter)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();
        return View(list);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> ResolveReport(int id, string status, string? adminNote)
    {
        var r = await _db.Reports.FindAsync(id);
        if (r == null) return NotFound();
        r.Status = status;
        r.AdminNote = adminNote;
        await _db.SaveChangesAsync();
        return RedirectToAction(nameof(Reports));
    }

    [HttpGet]
    public async Task<IActionResult> AiLogs()
    {
        var logs = await _db.AiLogs.AsNoTracking().OrderByDescending(l => l.CreatedAt).Take(100).ToListAsync();
        return View(logs);
    }
}
