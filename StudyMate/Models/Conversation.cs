namespace StudyMate.Models;

/// <summary>Hội thoại 1-1 giữa hai user (M6).</summary>
public class Conversation
{
    public int Id { get; set; }
    public string User1Id { get; set; } = string.Empty;
    public string User2Id { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ApplicationUser? User1 { get; set; }
    public ApplicationUser? User2 { get; set; }
    public ICollection<ChatMessage> Messages { get; set; } = new List<ChatMessage>();
}

/// <summary>Tin nhắn trong conversation (table Messages).</summary>
public class ChatMessage
{
    public int Id { get; set; }
    public int ConversationId { get; set; }
    public string SenderId { get; set; } = string.Empty;
    public string? Content { get; set; }
    public string? FileUrl { get; set; }
    public string? FileType { get; set; }
    public bool IsRead { get; set; }
    public bool IsFlagged { get; set; }
    public string? AiFlagNote { get; set; }
    public DateTime SentAt { get; set; } = DateTime.UtcNow;

    public Conversation? Conversation { get; set; }
    public ApplicationUser? Sender { get; set; }
}
