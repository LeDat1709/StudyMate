using System.ComponentModel.DataAnnotations;

namespace StudyMate.ViewModels.Account;

/// <summary>
/// ViewModel for the VerifyEmail OTP page.
/// </summary>
public class VerifyOtpViewModel
{
    /// <summary>Email being verified — passed as hidden field, not shown to user.</summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>6-digit OTP code entered by the user.</summary>
    [Required(ErrorMessage = "Vui lòng nhập mã OTP")]
    [StringLength(6, MinimumLength = 6, ErrorMessage = "Mã OTP phải đúng 6 chữ số")]
    [RegularExpression(@"^\d{6}$", ErrorMessage = "Mã OTP chỉ gồm 6 chữ số")]
    [Display(Name = "Mã OTP")]
    public string Code { get; set; } = string.Empty;
}
