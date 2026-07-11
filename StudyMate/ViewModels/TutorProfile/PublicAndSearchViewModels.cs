namespace StudyMate.ViewModels.TutorProfile;

public class PublicTutorProfileViewModel
{
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string? AvatarUrl { get; set; }
    public string? Headline { get; set; }
    public string? Bio { get; set; }
    public string? VideoIntroUrl { get; set; }
    public int? YearsOfExperience { get; set; }
    public string? EducationLevel { get; set; }
    public decimal? HourlyRate { get; set; }
    public string? TeachingMode { get; set; }
    public string? Address { get; set; }
    public decimal AverageRating { get; set; }
    public int TotalReviews { get; set; }
    public bool IsVerified { get; set; }
    public bool IsAvailable { get; set; }
    public List<string> Subjects { get; set; } = new();
    public List<PublicCertViewModel> Certificates { get; set; } = new();
    public List<AvailabilityItemViewModel> Availabilities { get; set; } = new();
    public List<DemoLessonItemViewModel> DemoLessons { get; set; } = new();
    public bool ShowContactButton { get; set; }
}

public class PublicCertViewModel
{
    public string Title { get; set; } = string.Empty;
    public string? CertType { get; set; }
    public string? IssuedBy { get; set; }
}

public class TutorSearchViewModel
{
    public int? SubjectId { get; set; }
    public decimal? MinRate { get; set; }
    public decimal? MaxRate { get; set; }
    public string? TeachingMode { get; set; }
    public string? EducationLevel { get; set; }
    public decimal? MinRating { get; set; }
    public string? Keyword { get; set; }
    public string Sort { get; set; } = "Newest";
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 12;
    public int TotalCount { get; set; }
    public int TotalPages => PageSize <= 0 ? 0 : (int)Math.Ceiling(TotalCount / (double)PageSize);
    public List<TutorSearchResultItem> Results { get; set; } = new();
    public List<SubjectOptionViewModel> Subjects { get; set; } = new();
}

public class TutorSearchResultItem
{
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string? AvatarUrl { get; set; }
    public string? Headline { get; set; }
    public decimal? HourlyRate { get; set; }
    public string? TeachingMode { get; set; }
    public string? EducationLevel { get; set; }
    public decimal AverageRating { get; set; }
    public int TotalReviews { get; set; }
    public List<string> Subjects { get; set; } = new();
}
