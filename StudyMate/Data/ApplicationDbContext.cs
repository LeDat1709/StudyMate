using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using StudyMate.Models;

namespace StudyMate.Data;

/// <summary>
/// DbContext chĂ­nh cá»§a StudyMate, káº¿ thá»«a IdentityDbContext Ä‘á»ƒ tĂ­ch há»£p ASP.NET Identity.
/// </summary>
public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    // â”€â”€ CĂ¡c DbSet sáº½ Ä‘Æ°á»£c thĂªm dáº§n theo tá»«ng module â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
    // Module 1
    public DbSet<OtpCode> OtpCodes { get; set; }

    // Module 2
    public DbSet<Subject> Subjects { get; set; }
    public DbSet<TutorProfile> TutorProfiles { get; set; }
    public DbSet<TutorSubject> TutorSubjects { get; set; }
    public DbSet<TutorCertificate> TutorCertificates { get; set; }
    public DbSet<TutorAvailability> TutorAvailabilities { get; set; }
    public DbSet<DemoLesson> DemoLessons { get; set; }

    // Module 3
    public DbSet<JobPosting> JobPostings { get; set; }

    // Module 4
    public DbSet<MatchingResult> MatchingResults { get; set; }

    // Module 5
    public DbSet<JobApplication> Applications { get; set; }

    // Module 6
    public DbSet<Conversation> Conversations { get; set; }
    public DbSet<ChatMessage> Messages { get; set; }

    // Module 7
    public DbSet<Booking> Bookings { get; set; }

    // Module 8
    public DbSet<UserReport> Reports { get; set; }
    public DbSet<AiLog> AiLogs { get; set; }

    // Module 9
    public DbSet<Wallet> Wallets { get; set; }
    public DbSet<WalletTransaction> Transactions { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // â”€â”€ OtpCode â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
        builder.Entity<OtpCode>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Code).HasMaxLength(10).IsRequired();
            e.Property(x => x.Purpose).HasMaxLength(50).IsRequired();
            e.HasOne<ApplicationUser>()
             .WithMany()
             .HasForeignKey(x => x.UserId)
             .OnDelete(DeleteBehavior.Cascade);
        });

        // â”€â”€ TutorCertificate (FK TutorProfile) â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
        builder.Entity<TutorCertificate>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Title).HasMaxLength(200).IsRequired();
            e.Property(x => x.IssuedBy).HasMaxLength(200);
            e.Property(x => x.FileUrl).HasMaxLength(500);
            e.Property(x => x.CertType).HasMaxLength(50);
            e.Property(x => x.IsVerified).HasDefaultValue(false);
            e.HasOne(x => x.TutorProfile)
             .WithMany(p => p.Certificates)
             .HasForeignKey(x => x.TutorProfileId)
             .OnDelete(DeleteBehavior.Cascade);
            e.HasIndex(x => x.TutorProfileId).HasDatabaseName("IX_TutorCertificates_TutorProfileId");
        });

        // â”€â”€ TutorAvailability â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
        builder.Entity<TutorAvailability>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasOne(x => x.TutorProfile)
             .WithMany(p => p.Availabilities)
             .HasForeignKey(x => x.TutorProfileId)
             .OnDelete(DeleteBehavior.Cascade);
            e.HasIndex(x => x.TutorProfileId).HasDatabaseName("IX_TutorAvailabilities_TutorProfileId");
        });

        // â”€â”€ DemoLesson â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
        builder.Entity<DemoLesson>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Title).HasMaxLength(200).IsRequired();
            e.Property(x => x.VideoUrl).HasMaxLength(500);
            e.HasOne(x => x.TutorProfile)
             .WithMany(p => p.DemoLessons)
             .HasForeignKey(x => x.TutorProfileId)
             .OnDelete(DeleteBehavior.Cascade);
            e.HasIndex(x => x.TutorProfileId).HasDatabaseName("IX_DemoLessons_TutorProfileId");
        });

        // â”€â”€ Subject â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
        builder.Entity<Subject>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Name).HasMaxLength(100).IsRequired();
            e.Property(x => x.Category).HasMaxLength(100);
            e.HasIndex(x => x.Name).IsUnique().HasDatabaseName("IX_Subjects_Name");
        });

        // â”€â”€ TutorProfile â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
        builder.Entity<TutorProfile>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.UserId).HasMaxLength(450).IsRequired();
            e.Property(x => x.Headline).HasMaxLength(200);
            e.Property(x => x.VideoIntroUrl).HasMaxLength(500);
            e.Property(x => x.EducationLevel).HasMaxLength(100);
            e.Property(x => x.TeachingMode).HasMaxLength(20);
            e.Property(x => x.Address).HasMaxLength(300);
            e.Property(x => x.HourlyRate).HasPrecision(10, 2);
            e.Property(x => x.AverageRating).HasPrecision(3, 2).HasDefaultValue(0m);
            e.Property(x => x.IsVerified).HasDefaultValue(false);
            e.Property(x => x.IsAvailable).HasDefaultValue(true);
            e.Property(x => x.TotalReviews).HasDefaultValue(0);

            e.HasOne(x => x.User)
             .WithMany()
             .HasForeignKey(x => x.UserId)
             .OnDelete(DeleteBehavior.Cascade);

            e.HasIndex(x => x.UserId)
             .IsUnique()
             .HasDatabaseName("IX_TutorProfiles_UserId");

            e.HasIndex(x => new { x.IsAvailable, x.IsVerified })
             .HasDatabaseName("IX_TutorProfiles_IsAvailable");
        });

        // â”€â”€ TutorSubject (composite key) â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
        builder.Entity<TutorSubject>(e =>
        {
            e.HasKey(x => new { x.TutorProfileId, x.SubjectId });

            e.HasOne(x => x.TutorProfile)
             .WithMany(p => p.TutorSubjects)
             .HasForeignKey(x => x.TutorProfileId)
             .OnDelete(DeleteBehavior.Cascade);

            e.HasOne(x => x.Subject)
             .WithMany(s => s.TutorSubjects)
             .HasForeignKey(x => x.SubjectId)
             .OnDelete(DeleteBehavior.Cascade);
        });

        // â”€â”€ JobPosting (M3) â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
        builder.Entity<JobPosting>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.StudentId).HasMaxLength(450).IsRequired();
            e.Property(x => x.Title).HasMaxLength(200).IsRequired();
            e.Property(x => x.DesiredLevel).HasMaxLength(100);
            e.Property(x => x.TeachingMode).HasMaxLength(20);
            e.Property(x => x.Address).HasMaxLength(300);
            e.Property(x => x.Status).HasMaxLength(20).HasDefaultValue("Open");
            e.Property(x => x.BudgetMin).HasPrecision(10, 2);
            e.Property(x => x.BudgetMax).HasPrecision(10, 2);

            e.HasOne(x => x.Student)
             .WithMany()
             .HasForeignKey(x => x.StudentId)
             .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(x => x.Subject)
             .WithMany()
             .HasForeignKey(x => x.SubjectId)
             .OnDelete(DeleteBehavior.Restrict);

            e.HasIndex(x => x.StudentId).HasDatabaseName("IX_JobPostings_StudentId");
            e.HasIndex(x => new { x.Status, x.Deadline }).HasDatabaseName("IX_JobPostings_Status");
        });

        // â”€â”€ MatchingResult (M4) â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
        builder.Entity<MatchingResult>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.SimilarityScore).HasPrecision(5, 4);
            e.Property(x => x.ModelVersion).HasMaxLength(50);
            e.HasOne(x => x.JobPosting).WithMany().HasForeignKey(x => x.JobPostingId)
                .OnDelete(DeleteBehavior.SetNull);
            e.HasOne(x => x.Student).WithMany().HasForeignKey(x => x.StudentId)
                .OnDelete(DeleteBehavior.Cascade);
            e.HasOne(x => x.TutorProfile).WithMany().HasForeignKey(x => x.TutorProfileId)
                .OnDelete(DeleteBehavior.Cascade);
            e.HasIndex(x => x.JobPostingId);
        });

        // â”€â”€ JobApplication (M5) â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
        builder.Entity<JobApplication>(e =>
        {
            e.ToTable("Applications");
            e.HasKey(x => x.Id);
            e.Property(x => x.Status).HasMaxLength(20).HasDefaultValue("Pending");
            e.Property(x => x.ProposedRate).HasPrecision(10, 2);
            e.HasOne(x => x.JobPosting).WithMany().HasForeignKey(x => x.JobPostingId)
                .OnDelete(DeleteBehavior.Cascade);
            e.HasOne(x => x.Tutor).WithMany().HasForeignKey(x => x.TutorId)
                .OnDelete(DeleteBehavior.Restrict);
            e.HasIndex(x => new { x.JobPostingId, x.TutorId }).IsUnique();
        });

        // â”€â”€ Chat (M6) â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
        builder.Entity<Conversation>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasOne(x => x.User1).WithMany().HasForeignKey(x => x.User1Id)
                .OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.User2).WithMany().HasForeignKey(x => x.User2Id)
                .OnDelete(DeleteBehavior.Restrict);
        });
        builder.Entity<ChatMessage>(e =>
        {
            e.ToTable("Messages");
            e.HasKey(x => x.Id);
            e.Property(x => x.FileUrl).HasMaxLength(500);
            e.Property(x => x.FileType).HasMaxLength(20);
            e.HasOne(x => x.Conversation).WithMany(c => c.Messages)
                .HasForeignKey(x => x.ConversationId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(x => x.Sender).WithMany().HasForeignKey(x => x.SenderId)
                .OnDelete(DeleteBehavior.Restrict);
            e.HasIndex(x => new { x.ConversationId, x.SentAt });
        });

        // â”€â”€ Booking (M7) â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
        builder.Entity<Booking>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Status).HasMaxLength(20).HasDefaultValue("Pending");
            e.Property(x => x.MeetingUrl).HasMaxLength(500);
            e.HasOne(x => x.Application).WithMany().HasForeignKey(x => x.ApplicationId)
                .OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.Student).WithMany().HasForeignKey(x => x.StudentId)
                .OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.Tutor).WithMany().HasForeignKey(x => x.TutorId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // â”€â”€ Report + AiLog (M8) â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
        builder.Entity<UserReport>(e =>
        {
            e.ToTable("Reports");
            e.HasKey(x => x.Id);
            e.Property(x => x.TargetType).HasMaxLength(50);
            e.Property(x => x.Status).HasMaxLength(20).HasDefaultValue("Pending");
            e.HasOne(x => x.Reporter).WithMany().HasForeignKey(x => x.ReporterId)
                .OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.TargetUser).WithMany().HasForeignKey(x => x.TargetUserId)
                .OnDelete(DeleteBehavior.SetNull);
        });
        builder.Entity<AiLog>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Action).HasMaxLength(100).IsRequired();
            e.Property(x => x.ModelUsed).HasMaxLength(100);
            e.HasOne(x => x.User).WithMany().HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        // â”€â”€ Index tá»‘i Æ°u query phá»• biáº¿n â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
        builder.Entity<ApplicationUser>()
            .HasIndex(u => u.NormalizedEmail)
            .HasDatabaseName("IX_AspNetUsers_Email");
    }
}

