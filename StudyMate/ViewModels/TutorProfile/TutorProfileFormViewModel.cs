using System.ComponentModel.DataAnnotations;

namespace StudyMate.ViewModels.TutorProfile;

public class TutorProfileFormViewModel
{
    public int? Id { get; set; }

    [Required(ErrorMessage = "Vui lòng nhập tiêu đề hồ sơ")]
    [StringLength(200)]
    [Display(Name = "Tiêu đề (Headline)")]
    public string Headline { get; set; } = string.Empty;

    [Required(ErrorMessage = "Vui lòng nhập giới thiệu")]
    [MinLength(100, ErrorMessage = "Giới thiệu tối thiểu 100 ký tự")]
    [Display(Name = "Giới thiệu (Bio)")]
    public string Bio { get; set; } = string.Empty;

    [Required(ErrorMessage = "Vui lòng nhập số năm kinh nghiệm")]
    [Range(0, 80, ErrorMessage = "Kinh nghiệm từ 0 đến 80 năm")]
    [Display(Name = "Số năm kinh nghiệm")]
    public int YearsOfExperience { get; set; }

    [Required(ErrorMessage = "Vui lòng chọn trình độ")]
    [Display(Name = "Trình độ học vấn")]
    public string EducationLevel { get; set; } = string.Empty;

    [Required(ErrorMessage = "Vui lòng nhập học phí")]
    [Range(typeof(decimal), "1000", "100000000", ErrorMessage = "Học phí phải lớn hơn 0")]
    [Display(Name = "Học phí / giờ (VNĐ)")]
    public decimal HourlyRate { get; set; }

    [Required(ErrorMessage = "Vui lòng chọn hình thức dạy")]
    [Display(Name = "Hình thức dạy")]
    public string TeachingMode { get; set; } = "Online";

    [StringLength(300)]
    [Display(Name = "Địa chỉ dạy (Offline)")]
    public string? Address { get; set; }

    [Display(Name = "Đang nhận học viên")]
    public bool IsAvailable { get; set; } = true;

    // Display-only
    public bool IsVerified { get; set; }
    public string? AvatarUrl { get; set; }
    public string? FullName { get; set; }
    public string? Email { get; set; }
}
