using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StudyMate.Data;
using StudyMate.Models;

namespace StudyMate.Controllers;
[Authorize]
public class NotificationController : Controller
{
    private readonly ApplicationDbContext _db;
    private readonly UserManager<ApplicationUser> _users;
    public NotificationController(ApplicationDbContext db, UserManager<ApplicationUser> users) { _db = db; _users = users; }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var me = _users.GetUserId(User)!;
        var list = await _db.Notifications.AsNoTracking().Where(n => n.UserId == me)
            .OrderByDescending(n => n.CreatedAt).Take(100).ToListAsync();
        return View(list);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> MarkRead(int id)
    {
        var me = _users.GetUserId(User)!;
        var n = await _db.Notifications.FirstOrDefaultAsync(x => x.Id == id && x.UserId == me);
        if (n != null) { n.IsRead = true; await _db.SaveChangesAsync(); }
        return RedirectToAction(nameof(Index));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> MarkAllRead()
    {
        var me = _users.GetUserId(User)!;
        var list = await _db.Notifications.Where(n => n.UserId == me && !n.IsRead).ToListAsync();
        foreach (var n in list) n.IsRead = true;
        await _db.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }
}
