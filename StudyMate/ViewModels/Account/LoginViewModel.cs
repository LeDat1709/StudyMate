using System.ComponentModel.DataAnnotations;

namespace StudyMate.ViewModels.Account;

/// <summary>
/// ViewModel for the Login page.
/// </summary>
public class LoginViewModel
{
    /// <summary>User's email address.</summary>
    [Required(ErrorMessage = "Vui lòng nhập email")]
    [EmailAddress(ErrorMessage = "Email không hợp lệ")]
    [Display(Name = "Email")]
    public string Email { get; set; } = string.Empty;

    /// <summary>User's password.</summary>
    [Required(ErrorMessage = "Vui lòng nhập mật khẩu")]
    [DataType(DataType.Password)]
    [Display(Name = "Mật khẩu")]
    public string Password { get; set; } = string.Empty;

    /// <summary>Whether to persist the login cookie across browser sessions.</summary>
    [Display(Name = "Ghi nhớ đăng nhập")]
    public bool RememberMe { get; set; } = false;
}
