using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StudyMate.Data;
using StudyMate.Models;

namespace StudyMate.Controllers;

[Authorize]
public class ChatController : Controller
{
    private readonly ApplicationDbContext _db;
    private readonly UserManager<ApplicationUser> _users;

    public ChatController(ApplicationDbContext db, UserManager<ApplicationUser> users)
    {
        _db = db;
        _users = users;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var me = _users.GetUserId(User)!;
        var list = await _db.Conversations.AsNoTracking()
            .Include(c => c.User1)
            .Include(c => c.User2)
            .Where(c => c.User1Id == me || c.User2Id == me)
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync();
        ViewData["Me"] = me;
        return View(list);
    }

    [HttpGet]
    public async Task<IActionResult> Room(int id)
    {
        var me = _users.GetUserId(User)!;
        var conv = await _db.Conversations
            .Include(c => c.User1)
            .Include(c => c.User2)
            .FirstOrDefaultAsync(c => c.Id == id && (c.User1Id == me || c.User2Id == me));
        if (conv == null) return NotFound();

        var messages = await _db.Messages.AsNoTracking()
            .Include(m => m.Sender)
            .Where(m => m.ConversationId == id)
            .OrderBy(m => m.SentAt)
            .Take(200)
            .ToListAsync();

        var unread = await _db.Messages
            .Where(m => m.ConversationId == id && m.SenderId != me && !m.IsRead)
            .ToListAsync();
        foreach (var m in unread) m.IsRead = true;
        if (unread.Count > 0) await _db.SaveChangesAsync();

        ViewData["Me"] = me;
        ViewData["ConversationId"] = id;
        ViewData["Peer"] = conv.User1Id == me ? conv.User2?.FullName : conv.User1?.FullName;
        return View(messages);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Send(int conversationId, string content)
    {
        var me = _users.GetUserId(User)!;
        var conv = await _db.Conversations.FirstOrDefaultAsync(c =>
            c.Id == conversationId && (c.User1Id == me || c.User2Id == me));
        if (conv == null) return NotFound();
        if (string.IsNullOrWhiteSpace(content))
            return RedirectToAction(nameof(Room), new { id = conversationId });

        _db.Messages.Add(new ChatMessage
        {
            ConversationId = conversationId,
            SenderId = me,
            Content = content.Trim(),
            FileType = "text",
            SentAt = DateTime.UtcNow
        });
        await _db.SaveChangesAsync();
        return RedirectToAction(nameof(Room), new { id = conversationId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> StartWith(string userId)
    {
        var me = _users.GetUserId(User)!;
        if (string.IsNullOrWhiteSpace(userId) || me == userId) return BadRequest();

        var existing = await _db.Conversations.FirstOrDefaultAsync(c =>
            (c.User1Id == me && c.User2Id == userId) ||
            (c.User1Id == userId && c.User2Id == me));
        if (existing != null)
            return RedirectToAction(nameof(Room), new { id = existing.Id });

        var conv = new Conversation
        {
            User1Id = me,
            User2Id = userId,
            CreatedAt = DateTime.UtcNow
        };
        _db.Conversations.Add(conv);
        await _db.SaveChangesAsync();
        return RedirectToAction(nameof(Room), new { id = conv.Id });
    }
}
