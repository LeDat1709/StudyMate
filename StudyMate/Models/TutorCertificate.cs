namespace StudyMate.Models;

/// <summary>
/// Chứng chỉ / bằng cấp / IELTS của gia sư.
/// M1-T9: gắn tạm theo UserId (AspNetUsers). M2 có thể chuyển FK sang TutorProfileId.
/// </summary>
public class TutorCertificate
{
    public int Id { get; set; }

    /// <summary>Id user (Tutor) sở hữu chứng chỉ.</summary>
    public string UserId { get; set; } = string.Empty;

    /// <summary>Tên chứng chỉ (VD: IELTS 8.0, Bằng ĐH Ngoại ngữ).</summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>Đơn vị cấp.</summary>
    public string? IssuedBy { get; set; }

    /// <summary>Ngày cấp.</summary>
    public DateOnly? IssuedDate { get; set; }

    /// <summary>Đường dẫn file trong wwwroot (relative URL).</summary>
    public string? FileUrl { get; set; }

    /// <summary>Loại: Degree / Certificate / IELTS / TOEIC.</summary>
    public string? CertType { get; set; }

    /// <summary>Đã được Admin/AI xác minh chưa. Mặc định false.</summary>
    public bool IsVerified { get; set; } = false;

    /// <summary>Ghi chú kiểm duyệt AI (Module 12 / M2) — không set ở M1-T9.</summary>
    public string? AiVerifyNote { get; set; }

    /// <summary>Thời điểm upload.</summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
