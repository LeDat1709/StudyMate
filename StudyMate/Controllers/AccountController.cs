using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using StudyMate.Models;
using StudyMate.Services.Interfaces;
using StudyMate.ViewModels.Account;

namespace StudyMate.Controllers;

/// <summary>
/// Handles authentication and account management actions.
/// </summary>
public class AccountController : Controller
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly IEmailService _emailService;
    private readonly IOtpService _otpService;
    private readonly ILogger<AccountController> _logger;

    public AccountController(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        RoleManager<IdentityRole> roleManager,
        IEmailService emailService,
        IOtpService otpService,
        ILogger<AccountController> logger)
    {
        _userManager  = userManager;
        _signInManager = signInManager;
        _roleManager  = roleManager;
        _emailService = emailService;
        _otpService   = otpService;
        _logger       = logger;
    }

    // ── Register ─────────────────────────────────────────────────────────────

    /// <summary>Displays the registration form.</summary>
    [HttpGet]
    public IActionResult Register()
    {
        return View(new RegisterViewModel());
    }

    /// <summary>Processes the registration form submission.</summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        // Only allow Student or Tutor roles via registration
        if (model.Role != "Student" && model.Role != "Tutor")
        {
            ModelState.AddModelError(nameof(model.Role), "Vai trò không hợp lệ");
            return View(model);
        }

        // Check duplicate email
        var existingUser = await _userManager.FindByEmailAsync(model.Email);
        if (existingUser != null)
        {
            ModelState.AddModelError(nameof(model.Email), "Email này đã được sử dụng");
            return View(model);
        }

        // Create new user
        var user = new ApplicationUser
        {
            FullName         = model.FullName.Trim(),
            UserName         = model.Email.Trim().ToLower(),
            Email            = model.Email.Trim().ToLower(),
            IsActive         = true,
            IsEmailVerified  = false,
            EmailConfirmed   = false,
            CreatedAt        = DateTime.UtcNow
        };

        var createResult = await _userManager.CreateAsync(user, model.Password);
        if (!createResult.Succeeded)
        {
            foreach (var error in createResult.Errors)
                ModelState.AddModelError(string.Empty, error.Description);
            return View(model);
        }

        // Assign role
        await _userManager.AddToRoleAsync(user, model.Role);

        // Generate OTP and send email
        var otpCode = await _otpService.GenerateAndSaveOtpAsync(user.Id, "EmailVerify");

        var emailBody = $@"
            <div style='font-family:sans-serif;max-width:480px;margin:auto;padding:24px;border:1px solid #e5e7eb;border-radius:8px'>
                <h2 style='color:#1d4ed8'>Xác thực email StudyMate</h2>
                <p>Xin chào <strong>{user.FullName}</strong>,</p>
                <p>Mã OTP xác thực email của bạn là:</p>
                <div style='font-size:2rem;font-weight:bold;letter-spacing:0.3rem;color:#1d4ed8;text-align:center;padding:16px 0'>{otpCode}</div>
                <p style='color:#6b7280'>Mã có hiệu lực trong <strong>5 phút</strong>. Không chia sẻ mã này cho bất kỳ ai.</p>
                <hr style='border:none;border-top:1px solid #e5e7eb'/>
                <p style='color:#9ca3af;font-size:0.85rem'>StudyMate — Nền tảng kết nối gia sư</p>
            </div>";

        await _emailService.SendEmailAsync(
            to: user.Email,
            subject: "[StudyMate] Mã xác thực email của bạn",
            body: emailBody,
            isHtml: true);

        // Store email in TempData for VerifyEmail page
        TempData["RegisteredEmail"] = user.Email;

        _logger.LogInformation("New user registered: {Email}, Role: {Role}", user.Email, model.Role);

        return RedirectToAction(nameof(VerifyEmail));
    }

    // ── Login ─────────────────────────────────────────────────────────────────

    /// <summary>Displays the login form.</summary>
    [HttpGet]
    public IActionResult Login(string? returnUrl = null)
    {
        // Already authenticated — redirect away
        if (User.Identity?.IsAuthenticated == true)
            return RedirectToLocal(returnUrl);

        ViewData["ReturnUrl"] = returnUrl;
        return View(new LoginViewModel());
    }

    /// <summary>Processes login form submission.</summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;

        if (!ModelState.IsValid)
            return View(model);

        var user = await _userManager.FindByEmailAsync(model.Email);

        // Do not reveal whether email exists or not
        if (user == null)
        {
            ModelState.AddModelError(string.Empty, "Email hoặc mật khẩu không chính xác");
            return View(model);
        }

        if (!user.IsActive)
        {
            ModelState.AddModelError(string.Empty, "Tài khoản của bạn đã bị khóa. Vui lòng liên hệ admin để được hỗ trợ.");
            return View(model);
        }

        if (!user.EmailConfirmed)
        {
            TempData["RegisteredEmail"] = user.Email;
            ModelState.AddModelError(string.Empty, "Vui lòng xác thực email trước khi đăng nhập.");
            ViewData["ShowResendLink"] = true;
            return View(model);
        }

        var result = await _signInManager.PasswordSignInAsync(
            user.UserName!,
            model.Password,
            model.RememberMe,
            lockoutOnFailure: true);

        if (result.Succeeded)
        {
            _logger.LogInformation("User logged in: {Email}", user.Email);
            return RedirectToLocal(returnUrl);
        }

        if (result.IsLockedOut)
        {
            _logger.LogWarning("User account locked out: {Email}", user.Email);
            ModelState.AddModelError(string.Empty, "Tài khoản tạm khóa do nhập sai quá nhiều lần. Vui lòng thử lại sau.");
            return View(model);
        }

        ModelState.AddModelError(string.Empty, "Email hoặc mật khẩu không chính xác");
        return View(model);
    }

    // ── Logout ────────────────────────────────────────────────────────────────

    /// <summary>Signs the user out and redirects to home.</summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync();
        _logger.LogInformation("User signed out");
        return RedirectToAction("Index", "Home");
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private IActionResult RedirectToLocal(string? returnUrl)
    {
        if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            return Redirect(returnUrl);
        return RedirectToAction("Index", "Home");
    }

    /// <summary>Displays the OTP verification form.</summary>
    [HttpGet]
    public IActionResult VerifyEmail()
    {
        var email = TempData["RegisteredEmail"] as string;

        // If accessed directly without going through Register, redirect back
        if (string.IsNullOrEmpty(email))
            return RedirectToAction(nameof(Register));

        return View(new VerifyOtpViewModel { Email = email });
    }

    /// <summary>Processes OTP submission and verifies the user's email.</summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> VerifyEmail(VerifyOtpViewModel model)
    {
        if (!ModelState.IsValid)
        {
            TempData["RegisteredEmail"] = model.Email;
            return View(model);
        }

        var user = await _userManager.FindByEmailAsync(model.Email);
        if (user == null)
        {
            ModelState.AddModelError(string.Empty, "Không tìm thấy tài khoản");
            TempData["RegisteredEmail"] = model.Email;
            return View(model);
        }

        var result = await _otpService.ValidateOtpAsync(user.Id, model.Code, "EmailVerify");

        switch (result)
        {
            case OtpValidationResult.Valid:
                user.EmailConfirmed   = true;
                user.IsEmailVerified  = true;
                user.UpdatedAt        = DateTime.UtcNow;
                await _userManager.UpdateAsync(user);
                await _signInManager.SignInAsync(user, isPersistent: false);
                _logger.LogInformation("Email verified and signed in: {Email}", user.Email);
                return RedirectToAction("Index", "Home");

            case OtpValidationResult.Expired:
                ModelState.AddModelError(nameof(model.Code), "Mã OTP đã hết hạn. Vui lòng nhấn \"Gửi lại OTP\".");
                break;

            case OtpValidationResult.AlreadyUsed:
                ModelState.AddModelError(nameof(model.Code), "Mã OTP đã được sử dụng. Vui lòng nhấn \"Gửi lại OTP\".");
                break;

            case OtpValidationResult.TooManyAttempts:
                ModelState.AddModelError(nameof(model.Code), "Bạn đã nhập sai quá nhiều lần. Vui lòng nhấn \"Gửi lại OTP\" để lấy mã mới.");
                break;

            default: // Invalid
                ModelState.AddModelError(nameof(model.Code), "Mã OTP không chính xác");
                break;
        }

        TempData["RegisteredEmail"] = model.Email;
        return View(model);
    }

    /// <summary>Invalidates old OTP and sends a new one to the user's email.</summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ResendOtp(string email)
    {
        if (string.IsNullOrEmpty(email))
            return RedirectToAction(nameof(Register));

        var user = await _userManager.FindByEmailAsync(email);
        if (user == null)
            return RedirectToAction(nameof(Register));

        // Already verified — no need to resend
        if (user.EmailConfirmed)
            return RedirectToAction("Index", "Home");

        var otpCode = await _otpService.GenerateAndSaveOtpAsync(user.Id, "EmailVerify");

        var emailBody = $@"
            <div style='font-family:sans-serif;max-width:480px;margin:auto;padding:24px;border:1px solid #e5e7eb;border-radius:8px'>
                <h2 style='color:#1d4ed8'>Xác thực email StudyMate</h2>
                <p>Xin chào <strong>{user.FullName}</strong>,</p>
                <p>Mã OTP xác thực email mới của bạn là:</p>
                <div style='font-size:2rem;font-weight:bold;letter-spacing:0.3rem;color:#1d4ed8;text-align:center;padding:16px 0'>{otpCode}</div>
                <p style='color:#6b7280'>Mã có hiệu lực trong <strong>5 phút</strong>. Không chia sẻ mã này cho bất kỳ ai.</p>
                <hr style='border:none;border-top:1px solid #e5e7eb'/>
                <p style='color:#9ca3af;font-size:0.85rem'>StudyMate — Nền tảng kết nối gia sư</p>
            </div>";

        await _emailService.SendEmailAsync(
            to: user.Email!,
            subject: "[StudyMate] Mã xác thực email mới của bạn",
            body: emailBody,
            isHtml: true);

        TempData["RegisteredEmail"] = email;
        TempData["ResendSuccess"]   = true;

        _logger.LogInformation("OTP resent to: {Email}", email);

        return RedirectToAction(nameof(VerifyEmail));
    }
}
