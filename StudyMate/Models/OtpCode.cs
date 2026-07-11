namespace StudyMate.Models;

/// <summary>
/// Lưu mã OTP dùng cho xác thực email và reset mật khẩu.
/// </summary>
public class OtpCode
{
    public int Id { get; set; }

    /// <summary>UserId của người dùng liên quan.</summary>
    public string UserId { get; set; } = string.Empty;

    /// <summary>Mã OTP 6 chữ số.</summary>
    public string Code { get; set; } = string.Empty;

    /// <summary>Mục đích: EmailVerify | ForgotPassword.</summary>
    public string Purpose { get; set; } = string.Empty;

    /// <summary>Thời điểm hết hạn.</summary>
    public DateTime ExpiredAt { get; set; }

    /// <summary>Đã sử dụng chưa.</summary>
    public bool IsUsed { get; set; } = false;

    /// <summary>Số lần nhập sai mã OTP. Tối đa 3 lần.</summary>
    public int FailedAttempts { get; set; } = 0;

    /// <summary>Thời điểm tạo.</summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
