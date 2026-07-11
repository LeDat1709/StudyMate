using System.ComponentModel.DataAnnotations;
using StudyMate.Models;

namespace StudyMate.ViewModels.JobPosting;

public class JobPostingFormViewModel
{
    public int? Id { get; set; }

    [Required(ErrorMessage = "Vui lòng nhập tiêu đề")]
    [StringLength(200)]
    [Display(Name = "Tiêu đề")]
    public string Title { get; set; } = string.Empty;

    [Required(ErrorMessage = "Vui lòng nhập mô tả")]
    [MinLength(50, ErrorMessage = "Mô tả tối thiểu 50 ký tự")]
    [Display(Name = "Mô tả chi tiết")]
    public string Description { get; set; } = string.Empty;

    [Required(ErrorMessage = "Chọn môn học")]
    [Display(Name = "Môn học")]
    public int SubjectId { get; set; }

    [Display(Name = "Trình độ mong muốn")]
    [StringLength(100)]
    public string? DesiredLevel { get; set; }

    [Required]
    [Display(Name = "Hình thức học")]
    public string TeachingMode { get; set; } = "Online";

    [Display(Name = "Địa điểm")]
    [StringLength(300)]
    public string? Address { get; set; }

    [Display(Name = "Ngân sách tối thiểu")]
    public decimal? BudgetMin { get; set; }

    [Display(Name = "Ngân sách tối đa")]
    public decimal? BudgetMax { get; set; }

    [Display(Name = "Số buổi / tuần")]
    [Range(1, 20)]
    public int? SessionsPerWeek { get; set; }

    [Display(Name = "Thời lượng buổi (phút)")]
    [Range(15, 480)]
    public int? SessionDuration { get; set; }

    [Display(Name = "Deadline nhận apply")]
    [DataType(DataType.Date)]
    public DateTime? Deadline { get; set; }

    public List<Subject> Subjects { get; set; } = new();
}

public class JobCardViewModel
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? SubjectName { get; set; }
    public string? TeachingMode { get; set; }
    public decimal? BudgetMin { get; set; }
    public decimal? BudgetMax { get; set; }
    public DateTime? Deadline { get; set; }
    public string Status { get; set; } = "Open";
    public DateTime CreatedAt { get; set; }
    public string? StudentName { get; set; }
}

public class JobListViewModel
{
    public List<JobCardViewModel> Items { get; set; } = new();
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public int TotalCount { get; set; }
    public int TotalPages => PageSize <= 0 ? 0 : (int)Math.Ceiling(TotalCount / (double)PageSize);
    public string? Keyword { get; set; }
    public int? SubjectId { get; set; }
    public string? TeachingMode { get; set; }
    public decimal? BudgetMin { get; set; }
    public decimal? BudgetMax { get; set; }
    public string? Address { get; set; }
    public string Sort { get; set; } = "Newest";
    public List<Subject> Subjects { get; set; } = new();
}

public class JobDetailViewModel
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? SubjectName { get; set; }
    public string? DesiredLevel { get; set; }
    public string? TeachingMode { get; set; }
    public string? Address { get; set; }
    public decimal? BudgetMin { get; set; }
    public decimal? BudgetMax { get; set; }
    public int? SessionsPerWeek { get; set; }
    public int? SessionDuration { get; set; }
    public DateTime? Deadline { get; set; }
    public string Status { get; set; } = "Open";
    public DateTime CreatedAt { get; set; }
    public string StudentId { get; set; } = string.Empty;
    public string? StudentName { get; set; }
    public string? StudentAvatar { get; set; }
    public bool IsOwner { get; set; }
    /// <summary>M5: show Apply for Tutor when Open.</summary>
    public bool ShowApplyPlaceholder { get; set; }

    public List<MatchedTutorItem> MatchedTutors { get; set; } = new();
}

public class MatchedTutorItem
{
    public int TutorProfileId { get; set; }
    public string? FullName { get; set; }
    public string? Headline { get; set; }
    public decimal Score { get; set; }
    public int Rank { get; set; }
}
