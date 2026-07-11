using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StudyMate.Data;
using StudyMate.Models;

namespace StudyMate.Controllers;

[Authorize]
public class WalletController : Controller
{
    private readonly ApplicationDbContext _db;
    private readonly UserManager<ApplicationUser> _users;
    public WalletController(ApplicationDbContext db, UserManager<ApplicationUser> users) { _db = db; _users = users; }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var me = _users.GetUserId(User)!;
        var wallet = await GetOrCreateAsync(me);
        var txs = await _db.Transactions.AsNoTracking().Where(t => t.WalletId == wallet.Id)
            .OrderByDescending(t => t.CreatedAt).Take(50).ToListAsync();
        ViewData["Balance"] = wallet.Balance;
        return View(txs);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Deposit(decimal amount)
    {
        if (amount <= 0) { TempData["Error"] = "Số tiền không hợp lệ."; return RedirectToAction(nameof(Index)); }
        var me = _users.GetUserId(User)!;
        var wallet = await GetOrCreateAsync(me);
        wallet.Balance += amount;
        wallet.UpdatedAt = DateTime.UtcNow;
        _db.Transactions.Add(new WalletTransaction {
            WalletId = wallet.Id, Amount = amount, Type = "Deposit", Gateway = "Stub",
            GatewayRef = Guid.NewGuid().ToString("N")[..12], Status = "Success",
            Note = "Nạp demo (stub)", CreatedAt = DateTime.UtcNow
        });
        await _db.SaveChangesAsync();
        TempData["Success"] = $"Đã nạp {amount:N0} ₫ (stub).";
        return RedirectToAction(nameof(Index));
    }

    private async Task<Wallet> GetOrCreateAsync(string userId)
    {
        var w = await _db.Wallets.FirstOrDefaultAsync(x => x.UserId == userId);
        if (w != null) return w;
        w = new Wallet { UserId = userId, Balance = 0, UpdatedAt = DateTime.UtcNow };
        _db.Wallets.Add(w);
        await _db.SaveChangesAsync();
        return w;
    }
}
