using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StudyMate.Data;
using StudyMate.Models;

namespace StudyMate.Controllers;

[Authorize]
public class BookingController : Controller
{
    private readonly ApplicationDbContext _db;
    private readonly UserManager<ApplicationUser> _users;

    public BookingController(ApplicationDbContext db, UserManager<ApplicationUser> users)
    {
        _db = db;
        _users = users;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var me = _users.GetUserId(User)!;
        var list = await _db.Bookings.AsNoTracking()
            .Include(b => b.Student)
            .Include(b => b.Tutor)
            .Where(b => b.StudentId == me || b.TutorId == me)
            .OrderByDescending(b => b.ScheduledStart)
            .ToListAsync();
        return View(list);
    }

    [Authorize(Roles = "Student")]
    [HttpGet]
    public async Task<IActionResult> Create(int applicationId)
    {
        var me = _users.GetUserId(User)!;
        var app = await _db.Applications.Include(a => a.JobPosting)
            .FirstOrDefaultAsync(a => a.Id == applicationId && a.Status == "Accepted");
        if (app?.JobPosting == null || app.JobPosting.StudentId != me) return NotFound();

        ViewData["JobTitle"] = app.JobPosting.Title;
        return View(new BookingFormVm
        {
            ApplicationId = applicationId,
            ScheduledStart = DateTime.Now.AddDays(1).Date.AddHours(18),
            ScheduledEnd = DateTime.Now.AddDays(1).Date.AddHours(19)
        });
    }

    [Authorize(Roles = "Student")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(BookingFormVm model)
    {
        var me = _users.GetUserId(User)!;
        var app = await _db.Applications.Include(a => a.JobPosting)
            .FirstOrDefaultAsync(a => a.Id == model.ApplicationId && a.Status == "Accepted");
        if (app?.JobPosting == null || app.JobPosting.StudentId != me) return NotFound();

        if (model.ScheduledEnd <= model.ScheduledStart)
            ModelState.AddModelError(nameof(model.ScheduledEnd), "Giờ kết thúc phải sau giờ bắt đầu.");
        if (!ModelState.IsValid)
        {
            ViewData["JobTitle"] = app.JobPosting.Title;
            return View(model);
        }

        _db.Bookings.Add(new Booking
        {
            ApplicationId = app.Id,
            StudentId = me,
            TutorId = app.TutorId,
            ScheduledStart = DateTime.SpecifyKind(model.ScheduledStart, DateTimeKind.Local).ToUniversalTime(),
            ScheduledEnd = DateTime.SpecifyKind(model.ScheduledEnd, DateTimeKind.Local).ToUniversalTime(),
            MeetingUrl = model.MeetingUrl?.Trim(),
            Note = model.Note?.Trim(),
            Status = "Pending",
            CreatedAt = DateTime.UtcNow
        });
        await _db.SaveChangesAsync();
        TempData["Success"] = "Đã tạo lịch học (chờ tutor confirm).";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Confirm(int id)
    {
        var me = _users.GetUserId(User)!;
        var b = await _db.Bookings.FirstOrDefaultAsync(x => x.Id == id && x.TutorId == me);
        if (b == null) return NotFound();
        if (b.Status != "Pending") return BadRequest();
        b.Status = "Confirmed";
        await _db.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Complete(int id)
    {
        var me = _users.GetUserId(User)!;
        var b = await _db.Bookings.FirstOrDefaultAsync(x => x.Id == id && (x.StudentId == me || x.TutorId == me));
        if (b == null) return NotFound();
        b.Status = "Completed";
        b.CheckOutAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Cancel(int id)
    {
        var me = _users.GetUserId(User)!;
        var b = await _db.Bookings.FirstOrDefaultAsync(x => x.Id == id && (x.StudentId == me || x.TutorId == me));
        if (b == null) return NotFound();
        b.Status = "Cancelled";
        await _db.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }
}

public class BookingFormVm
{
    public int ApplicationId { get; set; }

    [Required, Display(Name = "Bắt đầu")]
    public DateTime ScheduledStart { get; set; }

    [Required, Display(Name = "Kết thúc")]
    public DateTime ScheduledEnd { get; set; }

    [Display(Name = "Meeting URL")]
    public string? MeetingUrl { get; set; }

    [Display(Name = "Ghi chú")]
    public string? Note { get; set; }
}
