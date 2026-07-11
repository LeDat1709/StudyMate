namespace StudyMate.Models;

/// <summary>Báo cáo / khiếu nại (table Reports) — M8.</summary>
public class UserReport
{
    public int Id { get; set; }
    public string ReporterId { get; set; } = string.Empty;
    public string? TargetUserId { get; set; }
    public string? TargetType { get; set; }
    public int? TargetId { get; set; }
    public string Reason { get; set; } = string.Empty;
    public string Status { get; set; } = "Pending";
    public string? AdminNote { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ApplicationUser? Reporter { get; set; }
    public ApplicationUser? TargetUser { get; set; }
}

/// <summary>Log hành động AI — M8/M12.</summary>
public class AiLog
{
    public int Id { get; set; }
    public string? UserId { get; set; }
    public string Action { get; set; } = string.Empty;
    public string? InputData { get; set; }
    public string? OutputData { get; set; }
    public string? ModelUsed { get; set; }
    public int? DurationMs { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ApplicationUser? User { get; set; }
}
