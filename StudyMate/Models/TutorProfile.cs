namespace StudyMate.Models;

/// <summary>
/// Hồ sơ gia sư (1-1 với ApplicationUser role Tutor).
/// </summary>
public class TutorProfile
{
    public int Id { get; set; }

    /// <summary>FK AspNetUsers — unique, mỗi Tutor một hồ sơ.</summary>
    public string UserId { get; set; } = string.Empty;

    /// <summary>Tiêu đề ngắn, VD: "Gia sư IELTS 8.0+".</summary>
    public string? Headline { get; set; }

    /// <summary>Giới thiệu bản thân.</summary>
    public string? Bio { get; set; }

    /// <summary>URL YouTube/Vimeo hoặc path file MP4 local.</summary>
    public string? VideoIntroUrl { get; set; }

    /// <summary>Số năm kinh nghiệm (≥ 0).</summary>
    public int? YearsOfExperience { get; set; }

    /// <summary>Cử nhân / Thạc sĩ / Tiến sĩ / Khác.</summary>
    public string? EducationLevel { get; set; }

    /// <summary>Học phí / giờ (VNĐ).</summary>
    public decimal? HourlyRate { get; set; }

    /// <summary>Online / Offline / Both.</summary>
    public string? TeachingMode { get; set; }

    /// <summary>Địa chỉ dạy offline (bắt buộc nếu Offline/Both — validate ở UI).</summary>
    public string? Address { get; set; }

    /// <summary>Admin đã duyệt hồ sơ công khai chưa.</summary>
    public bool IsVerified { get; set; }

    /// <summary>Đang nhận học viên / tạm dừng.</summary>
    public bool IsAvailable { get; set; } = true;

    /// <summary>Điểm TB (M10 cập nhật). Default 0.</summary>
    public decimal AverageRating { get; set; }

    /// <summary>Số review (M10). Default 0.</summary>
    public int TotalReviews { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }

    public ApplicationUser? User { get; set; }

    public ICollection<TutorSubject> TutorSubjects { get; set; } = new List<TutorSubject>();
}
