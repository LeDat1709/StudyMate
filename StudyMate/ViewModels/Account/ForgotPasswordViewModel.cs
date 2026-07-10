using System.ComponentModel.DataAnnotations;

namespace StudyMate.ViewModels.Account;

/// <summary>
/// ViewModel for the Forgot Password page (step 1 — enter email).
/// </summary>
public class ForgotPasswordViewModel
{
    [Required(ErrorMessage = "Vui lòng nhập email")]
    [EmailAddress(ErrorMessage = "Email không hợp lệ")]
    [Display(Name = "Email")]
    public string Email { get; set; } = string.Empty;
}
