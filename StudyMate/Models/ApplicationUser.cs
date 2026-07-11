using Microsoft.AspNetCore.Identity;

namespace StudyMate.Models;

/// <summary>
/// Mở rộng IdentityUser với các field nghiệp vụ của StudyMate.
/// </summary>
public class ApplicationUser : IdentityUser
{
    /// <summary>Họ và tên đầy đủ.</summary>
    public string FullName { get; set; } = string.Empty;

    /// <summary>Đường dẫn ảnh đại diện.</summary>
    public string? AvatarUrl { get; set; }

    /// <summary>Ngày sinh.</summary>
    public DateOnly? DateOfBirth { get; set; }

    /// <summary>Giới tính: Male / Female / Other.</summary>
    public string? Gender { get; set; }

    /// <summary>Địa chỉ.</summary>
    public string? Address { get; set; }

    /// <summary>Tài khoản có đang hoạt động không (Admin có thể khóa).</summary>
    public bool IsActive { get; set; } = true;

    /// <summary>Email đã xác thực qua OTP chưa.</summary>
    public bool IsEmailVerified { get; set; } = false;

    /// <summary>Ngày tạo tài khoản.</summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>Ngày cập nhật gần nhất.</summary>
    public DateTime? UpdatedAt { get; set; }
}
