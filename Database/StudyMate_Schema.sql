-- ============================================================
--  StudyMate Database Schema
--  Platform: SQL Server (dev) / PostgreSQL (deploy on Render)
--  Nhóm: ASP.NET Core MVC + EF Core + ASP.NET Identity
-- ============================================================

USE master;
GO

IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = 'StudyMate')
    CREATE DATABASE StudyMate;
GO

USE StudyMate;
GO

-- ============================================================
-- MODULE 1: ASP.NET IDENTITY (mở rộng từ IdentityUser)
-- ============================================================

-- Roles
CREATE TABLE AspNetRoles (
    Id              NVARCHAR(450)   NOT NULL PRIMARY KEY,
    Name            NVARCHAR(256)   NULL,
    NormalizedName  NVARCHAR(256)   NULL,
    ConcurrencyStamp NVARCHAR(MAX)  NULL
);

-- Users (ApplicationUser kế thừa IdentityUser)
CREATE TABLE AspNetUsers (
    Id                      NVARCHAR(450)   NOT NULL PRIMARY KEY,
    -- Mở rộng thêm
    FullName                NVARCHAR(100)   NOT NULL,
    AvatarUrl               NVARCHAR(500)   NULL,
    PhoneNumber             NVARCHAR(20)    NULL,
    DateOfBirth             DATE            NULL,
    Gender                  NVARCHAR(10)    NULL,           -- Male / Female / Other
    Address                 NVARCHAR(300)   NULL,
    IsActive                BIT             NOT NULL DEFAULT 1,
    IsEmailVerified         BIT             NOT NULL DEFAULT 0,
    CreatedAt               DATETIME2       NOT NULL DEFAULT GETDATE(),
    UpdatedAt               DATETIME2       NULL,
    -- Identity fields
    UserName                NVARCHAR(256)   NULL,
    NormalizedUserName      NVARCHAR(256)   NULL,
    Email                   NVARCHAR(256)   NULL,
    NormalizedEmail         NVARCHAR(256)   NULL,
    EmailConfirmed          BIT             NOT NULL DEFAULT 0,
    PasswordHash            NVARCHAR(MAX)   NULL,
    SecurityStamp           NVARCHAR(MAX)   NULL,
    ConcurrencyStamp        NVARCHAR(MAX)   NULL,
    PhoneNumberConfirmed    BIT             NOT NULL DEFAULT 0,
    TwoFactorEnabled        BIT             NOT NULL DEFAULT 0,
    LockoutEnd              DATETIMEOFFSET  NULL,
    LockoutEnabled          BIT             NOT NULL DEFAULT 0,
    AccessFailedCount       INT             NOT NULL DEFAULT 0
);

CREATE TABLE AspNetUserRoles (
    UserId  NVARCHAR(450) NOT NULL,
    RoleId  NVARCHAR(450) NOT NULL,
    PRIMARY KEY (UserId, RoleId),
    FOREIGN KEY (UserId) REFERENCES AspNetUsers(Id) ON DELETE CASCADE,
    FOREIGN KEY (RoleId) REFERENCES AspNetRoles(Id) ON DELETE CASCADE
);

CREATE TABLE AspNetUserClaims (
    Id          INT             NOT NULL IDENTITY PRIMARY KEY,
    UserId      NVARCHAR(450)   NOT NULL,
    ClaimType   NVARCHAR(MAX)   NULL,
    ClaimValue  NVARCHAR(MAX)   NULL,
    FOREIGN KEY (UserId) REFERENCES AspNetUsers(Id) ON DELETE CASCADE
);

CREATE TABLE AspNetUserLogins (
    LoginProvider       NVARCHAR(128)   NOT NULL,
    ProviderKey         NVARCHAR(128)   NOT NULL,
    ProviderDisplayName NVARCHAR(MAX)   NULL,
    UserId              NVARCHAR(450)   NOT NULL,
    PRIMARY KEY (LoginProvider, ProviderKey),
    FOREIGN KEY (UserId) REFERENCES AspNetUsers(Id) ON DELETE CASCADE
);

CREATE TABLE AspNetUserTokens (
    UserId          NVARCHAR(450)   NOT NULL,
    LoginProvider   NVARCHAR(128)   NOT NULL,
    Name            NVARCHAR(128)   NOT NULL,
    Value           NVARCHAR(MAX)   NULL,
    PRIMARY KEY (UserId, LoginProvider, Name),
    FOREIGN KEY (UserId) REFERENCES AspNetUsers(Id) ON DELETE CASCADE
);

CREATE TABLE AspNetRoleClaims (
    Id          INT             NOT NULL IDENTITY PRIMARY KEY,
    RoleId      NVARCHAR(450)   NOT NULL,
    ClaimType   NVARCHAR(MAX)   NULL,
    ClaimValue  NVARCHAR(MAX)   NULL,
    FOREIGN KEY (RoleId) REFERENCES AspNetRoles(Id) ON DELETE CASCADE
);
GO

-- ============================================================
-- OTP Email Verification
-- ============================================================

CREATE TABLE OtpCodes (
    Id              INT             NOT NULL IDENTITY PRIMARY KEY,
    UserId          NVARCHAR(450)   NOT NULL,
    Code            NVARCHAR(10)    NOT NULL,
    Purpose         NVARCHAR(50)    NOT NULL,   -- EmailVerify / ForgotPassword
    ExpiredAt       DATETIME2       NOT NULL,
    IsUsed          BIT             NOT NULL DEFAULT 0,
    FailedAttempts  INT             NOT NULL DEFAULT 0,  -- số lần nhập sai, tối đa 3
    CreatedAt       DATETIME2       NOT NULL DEFAULT GETDATE(),
    FOREIGN KEY (UserId) REFERENCES AspNetUsers(Id) ON DELETE CASCADE
);
GO

-- ============================================================
-- MODULE 2: HỒ SƠ GIA SƯ (TutorProfile)
-- ============================================================

CREATE TABLE Subjects (
    Id      INT             NOT NULL IDENTITY PRIMARY KEY,
    Name    NVARCHAR(100)   NOT NULL,   -- Toán, IELTS, Lập trình...
    Category NVARCHAR(100)  NULL        -- Ngoại ngữ, THPT, Đại học...
);

CREATE TABLE TutorProfiles (
    Id                  INT             NOT NULL IDENTITY PRIMARY KEY,
    UserId              NVARCHAR(450)   NOT NULL UNIQUE,
    Headline            NVARCHAR(200)   NULL,           -- VD: "Gia sư IELTS 8.0+"
    Bio                 NVARCHAR(MAX)   NULL,
    VideoIntroUrl       NVARCHAR(500)   NULL,
    YearsOfExperience   INT             NULL,
    EducationLevel      NVARCHAR(100)   NULL,           -- Cử nhân / Thạc sĩ...
    HourlyRate          DECIMAL(10,2)   NULL,
    TeachingMode        NVARCHAR(20)    NULL,           -- Online / Offline / Both
    Address             NVARCHAR(300)   NULL,
    IsVerified          BIT             NOT NULL DEFAULT 0,
    IsAvailable         BIT             NOT NULL DEFAULT 1,
    AverageRating       DECIMAL(3,2)    NOT NULL DEFAULT 0,
    TotalReviews        INT             NOT NULL DEFAULT 0,
    CreatedAt           DATETIME2       NOT NULL DEFAULT GETDATE(),
    UpdatedAt           DATETIME2       NULL,
    FOREIGN KEY (UserId) REFERENCES AspNetUsers(Id) ON DELETE CASCADE
);

-- Môn dạy của gia sư (many-to-many)
CREATE TABLE TutorSubjects (
    TutorProfileId  INT NOT NULL,
    SubjectId       INT NOT NULL,
    PRIMARY KEY (TutorProfileId, SubjectId),
    FOREIGN KEY (TutorProfileId) REFERENCES TutorProfiles(Id) ON DELETE CASCADE,
    FOREIGN KEY (SubjectId)      REFERENCES Subjects(Id) ON DELETE CASCADE
);

-- Chứng chỉ / Bằng cấp / IELTS
CREATE TABLE TutorCertificates (
    Id              INT             NOT NULL IDENTITY PRIMARY KEY,
    TutorProfileId  INT             NOT NULL,
    Title           NVARCHAR(200)   NOT NULL,   -- IELTS 8.0 / Bằng ĐH...
    IssuedBy        NVARCHAR(200)   NULL,
    IssuedDate      DATE            NULL,
    FileUrl         NVARCHAR(500)   NULL,
    CertType        NVARCHAR(50)    NULL,       -- Degree / Certificate / IELTS / TOEIC
    IsVerified      BIT             NOT NULL DEFAULT 0,
    AiVerifyNote    NVARCHAR(MAX)   NULL,       -- AI kiểm duyệt ghi chú
    CreatedAt       DATETIME2       NOT NULL DEFAULT GETDATE(),
    FOREIGN KEY (TutorProfileId) REFERENCES TutorProfiles(Id) ON DELETE CASCADE
);

-- Lịch rảnh của gia sư
CREATE TABLE TutorAvailabilities (
    Id              INT             NOT NULL IDENTITY PRIMARY KEY,
    TutorProfileId  INT             NOT NULL,
    DayOfWeek       INT             NOT NULL,   -- 0=Sun ... 6=Sat
    StartTime       TIME            NOT NULL,
    EndTime         TIME            NOT NULL,
    FOREIGN KEY (TutorProfileId) REFERENCES TutorProfiles(Id) ON DELETE CASCADE
);

-- Demo lesson
CREATE TABLE DemoLessons (
    Id              INT             NOT NULL IDENTITY PRIMARY KEY,
    TutorProfileId  INT             NOT NULL,
    Title           NVARCHAR(200)   NOT NULL,
    Description     NVARCHAR(MAX)   NULL,
    VideoUrl        NVARCHAR(500)   NULL,
    CreatedAt       DATETIME2       NOT NULL DEFAULT GETDATE(),
    FOREIGN KEY (TutorProfileId) REFERENCES TutorProfiles(Id) ON DELETE CASCADE
);
GO

-- ============================================================
-- MODULE 3: YÊU CẦU THUÊ GIA SƯ (Job Posting)
-- ============================================================

CREATE TABLE JobPostings (
    Id                  INT             NOT NULL IDENTITY PRIMARY KEY,
    StudentId           NVARCHAR(450)   NOT NULL,       -- UserId của học viên
    Title               NVARCHAR(200)   NOT NULL,
    Description         NVARCHAR(MAX)   NULL,
    SubjectId           INT             NOT NULL,
    DesiredLevel        NVARCHAR(100)   NULL,           -- VD: IELTS 7.5+
    TeachingMode        NVARCHAR(20)    NULL,           -- Online / Offline / Both
    Address             NVARCHAR(300)   NULL,
    BudgetMin           DECIMAL(10,2)   NULL,
    BudgetMax           DECIMAL(10,2)   NULL,
    SessionsPerWeek     INT             NULL,
    SessionDuration     INT             NULL,           -- phút
    Deadline            DATETIME2       NULL,
    Status              NVARCHAR(20)    NOT NULL DEFAULT 'Open',  -- Open/Closed/Expired
    CreatedAt           DATETIME2       NOT NULL DEFAULT GETDATE(),
    UpdatedAt           DATETIME2       NULL,
    FOREIGN KEY (StudentId)  REFERENCES AspNetUsers(Id),
    FOREIGN KEY (SubjectId)  REFERENCES Subjects(Id)
);
GO

-- ============================================================
-- MODULE 4: AI MATCHING
-- ============================================================

CREATE TABLE MatchingResults (
    Id              INT             NOT NULL IDENTITY PRIMARY KEY,
    JobPostingId    INT             NULL,           -- NULL nếu gợi ý tổng quát
    StudentId       NVARCHAR(450)   NOT NULL,
    TutorProfileId  INT             NOT NULL,
    SimilarityScore DECIMAL(5,4)    NOT NULL,       -- 0.0 - 1.0
    Rank            INT             NOT NULL,
    ModelVersion    NVARCHAR(50)    NULL,           -- sentence-transformer version
    CreatedAt       DATETIME2       NOT NULL DEFAULT GETDATE(),
    FOREIGN KEY (StudentId)      REFERENCES AspNetUsers(Id),
    FOREIGN KEY (TutorProfileId) REFERENCES TutorProfiles(Id),
    FOREIGN KEY (JobPostingId)   REFERENCES JobPostings(Id)
);
GO

-- ============================================================
-- MODULE 5: QUY TRÌNH THUÊ (Application)
-- ============================================================

CREATE TABLE Applications (
    Id              INT             NOT NULL IDENTITY PRIMARY KEY,
    JobPostingId    INT             NOT NULL,
    TutorId         NVARCHAR(450)   NOT NULL,       -- UserId gia sư
    CoverNote       NVARCHAR(MAX)   NULL,
    ProposedRate    DECIMAL(10,2)   NULL,
    Status          NVARCHAR(20)    NOT NULL DEFAULT 'Pending',
                                                    -- Pending/Accepted/Rejected/Cancelled
    AppliedAt       DATETIME2       NOT NULL DEFAULT GETDATE(),
    UpdatedAt       DATETIME2       NULL,
    FOREIGN KEY (JobPostingId) REFERENCES JobPostings(Id),
    FOREIGN KEY (TutorId)      REFERENCES AspNetUsers(Id)
);
GO

-- ============================================================
-- MODULE 6: CHAT REALTIME (SignalR)
-- ============================================================

CREATE TABLE Conversations (
    Id          INT             NOT NULL IDENTITY PRIMARY KEY,
    User1Id     NVARCHAR(450)   NOT NULL,
    User2Id     NVARCHAR(450)   NOT NULL,
    CreatedAt   DATETIME2       NOT NULL DEFAULT GETDATE(),
    FOREIGN KEY (User1Id) REFERENCES AspNetUsers(Id),
    FOREIGN KEY (User2Id) REFERENCES AspNetUsers(Id)
);

CREATE TABLE Messages (
    Id              INT             NOT NULL IDENTITY PRIMARY KEY,
    ConversationId  INT             NOT NULL,
    SenderId        NVARCHAR(450)   NOT NULL,
    Content         NVARCHAR(MAX)   NULL,
    FileUrl         NVARCHAR(500)   NULL,
    FileType        NVARCHAR(20)    NULL,    -- text / image / pdf
    IsRead          BIT             NOT NULL DEFAULT 0,
    IsFlagged       BIT             NOT NULL DEFAULT 0,   -- AI phát hiện lừa đảo
    AiFlagNote      NVARCHAR(MAX)   NULL,
    SentAt          DATETIME2       NOT NULL DEFAULT GETDATE(),
    FOREIGN KEY (ConversationId) REFERENCES Conversations(Id) ON DELETE CASCADE,
    FOREIGN KEY (SenderId)       REFERENCES AspNetUsers(Id)
);
GO

-- ============================================================
-- MODULE 7: LỊCH HỌC (Booking / Calendar)
-- ============================================================

CREATE TABLE Bookings (
    Id              INT             NOT NULL IDENTITY PRIMARY KEY,
    ApplicationId   INT             NOT NULL,
    StudentId       NVARCHAR(450)   NOT NULL,
    TutorId         NVARCHAR(450)   NOT NULL,
    ScheduledStart  DATETIME2       NOT NULL,
    ScheduledEnd    DATETIME2       NOT NULL,
    MeetingUrl      NVARCHAR(500)   NULL,           -- link Google Meet / Zoom
    Status          NVARCHAR(20)    NOT NULL DEFAULT 'Pending',
                                                    -- Pending/Confirmed/Cancelled/Completed
    CheckInAt       DATETIME2       NULL,
    CheckOutAt      DATETIME2       NULL,
    Note            NVARCHAR(MAX)   NULL,
    CreatedAt       DATETIME2       NOT NULL DEFAULT GETDATE(),
    FOREIGN KEY (ApplicationId) REFERENCES Applications(Id),
    FOREIGN KEY (StudentId)     REFERENCES AspNetUsers(Id),
    FOREIGN KEY (TutorId)       REFERENCES AspNetUsers(Id)
);
GO

-- ============================================================
-- MODULE 9: THANH TOÁN
-- ============================================================

CREATE TABLE Wallets (
    Id          INT             NOT NULL IDENTITY PRIMARY KEY,
    UserId      NVARCHAR(450)   NOT NULL UNIQUE,
    Balance     DECIMAL(15,2)   NOT NULL DEFAULT 0,
    UpdatedAt   DATETIME2       NOT NULL DEFAULT GETDATE(),
    FOREIGN KEY (UserId) REFERENCES AspNetUsers(Id) ON DELETE CASCADE
);

CREATE TABLE Transactions (
    Id              INT             NOT NULL IDENTITY PRIMARY KEY,
    WalletId        INT             NOT NULL,
    BookingId       INT             NULL,
    Amount          DECIMAL(15,2)   NOT NULL,
    Type            NVARCHAR(20)    NOT NULL,    -- Deposit/Withdraw/Payment/Refund
    Gateway         NVARCHAR(30)    NULL,        -- VNPay/MoMo/Stripe/Wallet
    GatewayRef      NVARCHAR(200)   NULL,        -- mã giao dịch từ cổng thanh toán
    Status          NVARCHAR(20)    NOT NULL DEFAULT 'Pending',  -- Pending/Success/Failed
    Note            NVARCHAR(MAX)   NULL,
    CreatedAt       DATETIME2       NOT NULL DEFAULT GETDATE(),
    FOREIGN KEY (WalletId)  REFERENCES Wallets(Id),
    FOREIGN KEY (BookingId) REFERENCES Bookings(Id)
);
GO

-- ============================================================
-- MODULE 10: ĐÁNH GIÁ (Review & Rating)
-- ============================================================

CREATE TABLE Reviews (
    Id              INT             NOT NULL IDENTITY PRIMARY KEY,
    BookingId       INT             NOT NULL UNIQUE,
    ReviewerId      NVARCHAR(450)   NOT NULL,    -- Student
    TutorProfileId  INT             NOT NULL,
    Rating          TINYINT         NOT NULL,    -- 1-5
    Comment         NVARCHAR(MAX)   NULL,
    TutorReply      NVARCHAR(MAX)   NULL,
    IsSpam          BIT             NOT NULL DEFAULT 0,
    AiSpamNote      NVARCHAR(MAX)   NULL,
    CreatedAt       DATETIME2       NOT NULL DEFAULT GETDATE(),
    FOREIGN KEY (BookingId)       REFERENCES Bookings(Id),
    FOREIGN KEY (ReviewerId)      REFERENCES AspNetUsers(Id),
    FOREIGN KEY (TutorProfileId)  REFERENCES TutorProfiles(Id)
);
GO

-- ============================================================
-- MODULE 11: THÔNG BÁO (Notification)
-- ============================================================

CREATE TABLE Notifications (
    Id          INT             NOT NULL IDENTITY PRIMARY KEY,
    UserId      NVARCHAR(450)   NOT NULL,
    Title       NVARCHAR(200)   NOT NULL,
    Body        NVARCHAR(MAX)   NULL,
    Type        NVARCHAR(50)    NULL,        -- NewJob/NewApply/Booking/Chat/Payment/Rating
    ReferenceId INT             NULL,        -- Id của entity liên quan
    IsRead      BIT             NOT NULL DEFAULT 0,
    CreatedAt   DATETIME2       NOT NULL DEFAULT GETDATE(),
    FOREIGN KEY (UserId) REFERENCES AspNetUsers(Id) ON DELETE CASCADE
);
GO

-- ============================================================
-- MODULE 8 & 12: ADMIN + AI LOGGING
-- ============================================================

-- Báo cáo / Khiếu nại
CREATE TABLE Reports (
    Id              INT             NOT NULL IDENTITY PRIMARY KEY,
    ReporterId      NVARCHAR(450)   NOT NULL,
    TargetUserId    NVARCHAR(450)   NULL,
    TargetType      NVARCHAR(50)    NULL,    -- User/Review/Message/JobPosting
    TargetId        INT             NULL,
    Reason          NVARCHAR(MAX)   NOT NULL,
    Status          NVARCHAR(20)    NOT NULL DEFAULT 'Pending',  -- Pending/Resolved/Dismissed
    AdminNote       NVARCHAR(MAX)   NULL,
    CreatedAt       DATETIME2       NOT NULL DEFAULT GETDATE(),
    FOREIGN KEY (ReporterId)    REFERENCES AspNetUsers(Id),
    FOREIGN KEY (TargetUserId)  REFERENCES AspNetUsers(Id)
);

-- AI Activity Log
CREATE TABLE AiLogs (
    Id          INT             NOT NULL IDENTITY PRIMARY KEY,
    UserId      NVARCHAR(450)   NULL,
    Action      NVARCHAR(100)   NOT NULL,   -- Matching/CertVerify/SpamDetect/Chatbot/Quiz
    InputData   NVARCHAR(MAX)   NULL,
    OutputData  NVARCHAR(MAX)   NULL,
    ModelUsed   NVARCHAR(100)   NULL,
    DurationMs  INT             NULL,
    CreatedAt   DATETIME2       NOT NULL DEFAULT GETDATE(),
    FOREIGN KEY (UserId) REFERENCES AspNetUsers(Id)
);
GO

-- ============================================================
-- SEED DATA
-- ============================================================

-- Roles mặc định
INSERT INTO AspNetRoles (Id, Name, NormalizedName, ConcurrencyStamp) VALUES
    (NEWID(), 'Admin',   'ADMIN',   NEWID()),
    (NEWID(), 'Tutor',   'TUTOR',   NEWID()),
    (NEWID(), 'Student', 'STUDENT', NEWID()),
    (NEWID(), 'Guest',   'GUEST',   NEWID());

-- Môn học mẫu
INSERT INTO Subjects (Name, Category) VALUES
    (N'Toán',           N'THPT'),
    (N'Vật lý',         N'THPT'),
    (N'Hóa học',        N'THPT'),
    (N'Tiếng Anh',      N'Ngoại ngữ'),
    (N'IELTS',          N'Ngoại ngữ'),
    (N'TOEIC',          N'Ngoại ngữ'),
    (N'Lập trình C#',   N'Công nghệ'),
    (N'Lập trình Python',N'Công nghệ'),
    (N'Cơ sở dữ liệu',  N'Công nghệ'),
    (N'Văn học',        N'THPT'),
    (N'Lịch sử',        N'THPT'),
    (N'Địa lý',         N'THPT');
GO

-- ============================================================
-- INDEXES tối ưu query phổ biến
-- ============================================================

CREATE INDEX IX_AspNetUsers_Email          ON AspNetUsers(NormalizedEmail);
CREATE INDEX IX_TutorProfiles_UserId       ON TutorProfiles(UserId);
CREATE INDEX IX_TutorProfiles_IsAvailable  ON TutorProfiles(IsAvailable, IsVerified);
CREATE INDEX IX_JobPostings_StudentId      ON JobPostings(StudentId);
CREATE INDEX IX_JobPostings_Status         ON JobPostings(Status, Deadline);
CREATE INDEX IX_Applications_JobPostingId  ON Applications(JobPostingId);
CREATE INDEX IX_Applications_TutorId       ON Applications(TutorId);
CREATE INDEX IX_Bookings_StudentId         ON Bookings(StudentId);
CREATE INDEX IX_Bookings_TutorId           ON Bookings(TutorId);
CREATE INDEX IX_Messages_ConversationId    ON Messages(ConversationId, SentAt);
CREATE INDEX IX_Notifications_UserId       ON Notifications(UserId, IsRead);
CREATE INDEX IX_MatchingResults_StudentId  ON MatchingResults(StudentId);
GO

PRINT 'StudyMate schema created successfully.';
