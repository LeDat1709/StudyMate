using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace StudyMate.ViewModels.Account;

public class ProfileViewModel
{
    [Display(Name = "Email")]
    public string Email { get; set; } = string.Empty;

    [Display(Name = "Họ và tên")]
    [Required(ErrorMessage = "Vui lòng nhập họ và tên")]
    [MaxLength(100)]
    public string FullName { get; set; } = string.Empty;

    [Display(Name = "Số điện thoại")]
    [Phone(ErrorMessage = "Số điện thoại không hợp lệ")]
    public string? PhoneNumber { get; set; }

    [Display(Name = "Ảnh đại diện")]
    public string? AvatarUrl { get; set; }

    // For client-side upload via separate endpoint; not used by POST profile form
    public IFormFile? AvatarFile { get; set; }
}
