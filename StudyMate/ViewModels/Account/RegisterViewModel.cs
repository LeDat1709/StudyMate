using System.ComponentModel.DataAnnotations;

namespace StudyMate.ViewModels.Account;

/// <summary>
/// ViewModel for the Register page.
/// </summary>
public class RegisterViewModel
{
    /// <summary>Full name of the user.</summary>
    [Required(ErrorMessage = "Vui lòng nhập họ và tên")]
    [MaxLength(100, ErrorMessage = "Họ và tên không được vượt quá 100 ký tự")]
    [Display(Name = "Họ và tên")]
    public string FullName { get; set; } = string.Empty;

    /// <summary>Email address used for login.</summary>
    [Required(ErrorMessage = "Vui lòng nhập email")]
    [EmailAddress(ErrorMessage = "Email không hợp lệ")]
    [Display(Name = "Email")]
    public string Email { get; set; } = string.Empty;

    /// <summary>Password — min 8 chars, at least 1 uppercase, 1 digit.</summary>
    [Required(ErrorMessage = "Vui lòng nhập mật khẩu")]
    [MinLength(8, ErrorMessage = "Mật khẩu tối thiểu 8 ký tự")]
    [RegularExpression(@"^(?=.*[A-Z])(?=.*\d).+$",
        ErrorMessage = "Mật khẩu phải có ít nhất 1 chữ hoa và 1 chữ số")]
    [DataType(DataType.Password)]
    [Display(Name = "Mật khẩu")]
    public string Password { get; set; } = string.Empty;

    /// <summary>Must match Password.</summary>
    [Required(ErrorMessage = "Vui lòng xác nhận mật khẩu")]
    [Compare(nameof(Password), ErrorMessage = "Mật khẩu xác nhận không khớp")]
    [DataType(DataType.Password)]
    [Display(Name = "Xác nhận mật khẩu")]
    public string ConfirmPassword { get; set; } = string.Empty;

    /// <summary>Role selected by the user: Student or Tutor.</summary>
    [Required(ErrorMessage = "Vui lòng chọn vai trò")]
    [Display(Name = "Vai trò")]
    public string Role { get; set; } = "Student";
}
