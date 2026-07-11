using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using StudyMate.Data;
using StudyMate.Models;

namespace StudyMate.Controllers;

[Authorize]
public class AiAssistantController : Controller
{
    private readonly ApplicationDbContext _db;
    private readonly UserManager<ApplicationUser> _users;
    public AiAssistantController(ApplicationDbContext db, UserManager<ApplicationUser> users)
    { _db = db; _users = users; }

    [HttpGet]
    public IActionResult Index() => View(new AiChatVm());

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Ask(AiChatVm model)
    {
        var me = _users.GetUserId(User);
        var q = model.Question?.Trim() ?? "";
        if (string.IsNullOrEmpty(q))
        {
            ModelState.AddModelError(nameof(model.Question), "Nhập câu hỏi.");
            return View("Index", model);
        }
        var answer = q.ToLowerInvariant() switch
        {
            var s when s.Contains("ielts") => "Gợi ý: luyện Writing Task 2 3 lần/tuần, Reading timed tests, Speaking mock với tutor.",
            var s when s.Contains("gia sư") || s.Contains("tutor") => "Vào Tìm gia sư hoặc đăng Job Posting để nhận apply.",
            var s when s.Contains("học phí") => "So sánh HourlyRate trên hồ sơ gia sư với budget khi đăng job.",
            _ => $"StudyMate AI (stub): Đã nhận \"{q}\". Tích hợp LLM/Python sẽ trả lời chi tiết hơn."
        };
        _db.AiLogs.Add(new AiLog {
            UserId = me, Action = "Chatbot", InputData = q, OutputData = answer,
            ModelUsed = "stub-rule-v1", CreatedAt = DateTime.UtcNow
        });
        await _db.SaveChangesAsync();
        model.Answer = answer;
        return View("Index", model);
    }
}
public class AiChatVm { public string? Question { get; set; } public string? Answer { get; set; } }
