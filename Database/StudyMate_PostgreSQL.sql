-- ============================================================
--  StudyMate Database Schema - PostgreSQL
--  Platform: PostgreSQL (Deploy on Render)
--  Nhóm: ASP.NET Core MVC + EF Core + ASP.NET Identity
--
--  Khác biệt so với SQL Server:
--    NVARCHAR(n)   → VARCHAR(n)
--    NVARCHAR(MAX) → TEXT
--    BIT           → BOOLEAN
--    DATETIME2     → TIMESTAMPTZ
--    DATETIMEOFFSET→ TIMESTAMPTZ
--    IDENTITY      → GENERATED ALWAYS AS IDENTITY
--    TINYINT       → SMALLINT
--    GETDATE()     → NOW()
--    NEWID()       → gen_random_uuid()
--    GO            → (bỏ)
--    N'...'        → '...'
-- ============================================================

-- Tạo extension để dùng gen_random_uuid()
CREATE EXTENSION IF NOT EXISTS pgcrypto;

-- ============================================================
-- MODULE 1: ASP.NET IDENTITY
-- ============================================================

-- Roles
CREATE TABLE "AspNetRoles" (
    "Id"                VARCHAR(450)    NOT NULL PRIMARY KEY,
    "Name"              VARCHAR(256)    NULL,
    "NormalizedName"    VARCHAR(256)    NULL,
    "ConcurrencyStamp"  TEXT            NULL
);

-- Users (ApplicationUser kế thừa IdentityUser)
CREATE TABLE "AspNetUsers" (
    "Id"                        VARCHAR(450)    NOT NULL PRIMARY KEY,
    -- Mở rộng thêm
    "FullName"                  VARCHAR(100)    NOT NULL,
    "AvatarUrl"                 VARCHAR(500)    NULL,
    "PhoneNumber"               VARCHAR(20)     NULL,
    "DateOfBirth"               DATE            NULL,
    "Gender"                    VARCHAR(10)     NULL,           -- Male / Female / Other
    "Address"                   VARCHAR(300)    NULL,
    "IsActive"                  BOOLEAN         NOT NULL DEFAULT TRUE,
    "IsEmailVerified"           BOOLEAN         NOT NULL DEFAULT FALSE,
    "CreatedAt"                 TIMESTAMPTZ     NOT NULL DEFAULT NOW(),
    "UpdatedAt"                 TIMESTAMPTZ     NULL,
    -- Identity fields
    "UserName"                  VARCHAR(256)    NULL,
    "NormalizedUserName"        VARCHAR(256)    NULL,
    "Email"                     VARCHAR(256)    NULL,
    "NormalizedEmail"           VARCHAR(256)    NULL,
    "EmailConfirmed"            BOOLEAN         NOT NULL DEFAULT FALSE,
    "PasswordHash"              TEXT            NULL,
    "SecurityStamp"             TEXT            NULL,
    "ConcurrencyStamp"          TEXT            NULL,
    "PhoneNumberConfirmed"      BOOLEAN         NOT NULL DEFAULT FALSE,
    "TwoFactorEnabled"          BOOLEAN         NOT NULL DEFAULT FALSE,
    "LockoutEnd"                TIMESTAMPTZ     NULL,
    "LockoutEnabled"            BOOLEAN         NOT NULL DEFAULT FALSE,
    "AccessFailedCount"         INT             NOT NULL DEFAULT 0
);

CREATE TABLE "AspNetUserRoles" (
    "UserId"    VARCHAR(450)    NOT NULL,
    "RoleId"    VARCHAR(450)    NOT NULL,
    PRIMARY KEY ("UserId", "RoleId"),
    FOREIGN KEY ("UserId") REFERENCES "AspNetUsers"("Id") ON DELETE CASCADE,
    FOREIGN KEY ("RoleId") REFERENCES "AspNetRoles"("Id") ON DELETE CASCADE
);

CREATE TABLE "AspNetUserClaims" (
    "Id"            INT         NOT NULL GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    "UserId"        VARCHAR(450) NOT NULL,
    "ClaimType"     TEXT        NULL,
    "ClaimValue"    TEXT        NULL,
    FOREIGN KEY ("UserId") REFERENCES "AspNetUsers"("Id") ON DELETE CASCADE
);

CREATE TABLE "AspNetUserLogins" (
    "LoginProvider"         VARCHAR(128)    NOT NULL,
    "ProviderKey"           VARCHAR(128)    NOT NULL,
    "ProviderDisplayName"   TEXT            NULL,
    "UserId"                VARCHAR(450)    NOT NULL,
    PRIMARY KEY ("LoginProvider", "ProviderKey"),
    FOREIGN KEY ("UserId") REFERENCES "AspNetUsers"("Id") ON DELETE CASCADE
);

CREATE TABLE "AspNetUserTokens" (
    "UserId"        VARCHAR(450)    NOT NULL,
    "LoginProvider" VARCHAR(128)    NOT NULL,
    "Name"          VARCHAR(128)    NOT NULL,
    "Value"         TEXT            NULL,
    PRIMARY KEY ("UserId", "LoginProvider", "Name"),
    FOREIGN KEY ("UserId") REFERENCES "AspNetUsers"("Id") ON DELETE CASCADE
);

CREATE TABLE "AspNetRoleClaims" (
    "Id"            INT         NOT NULL GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    "RoleId"        VARCHAR(450) NOT NULL,
    "ClaimType"     TEXT        NULL,
    "ClaimValue"    TEXT        NULL,
    FOREIGN KEY ("RoleId") REFERENCES "AspNetRoles"("Id") ON DELETE CASCADE
);

-- ============================================================
-- OTP Email Verification
-- ============================================================

CREATE TABLE "OtpCodes" (
    "Id"                INT             NOT NULL GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    "UserId"            VARCHAR(450)    NOT NULL,
    "Code"              VARCHAR(10)     NOT NULL,
    "Purpose"           VARCHAR(50)     NOT NULL,   -- EmailVerify / ForgotPassword
    "ExpiredAt"         TIMESTAMPTZ     NOT NULL,
    "IsUsed"            BOOLEAN         NOT NULL DEFAULT FALSE,
    "FailedAttempts"    INT             NOT NULL DEFAULT 0,  -- số lần nhập sai, tối đa 3
    "CreatedAt"         TIMESTAMPTZ     NOT NULL DEFAULT NOW(),
    FOREIGN KEY ("UserId") REFERENCES "AspNetUsers"("Id") ON DELETE CASCADE
);

-- ============================================================
-- MODULE 2: HỒ SƠ GIA SƯ (TutorProfile)
-- ============================================================

CREATE TABLE "Subjects" (
    "Id"        INT             NOT NULL GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    "Name"      VARCHAR(100)    NOT NULL,   -- Toán, IELTS, Lập trình...
    "Category"  VARCHAR(100)    NULL        -- Ngoại ngữ, THPT, Đại học...
);

CREATE TABLE "TutorProfiles" (
    "Id"                    INT             NOT NULL GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    "UserId"                VARCHAR(450)    NOT NULL UNIQUE,
    "Headline"              VARCHAR(200)    NULL,           -- VD: "Gia sư IELTS 8.0+"
    "Bio"                   TEXT            NULL,
    "VideoIntroUrl"         VARCHAR(500)    NULL,
    "YearsOfExperience"     INT             NULL,
    "EducationLevel"        VARCHAR(100)    NULL,           -- Cử nhân / Thạc sĩ...
    "HourlyRate"            DECIMAL(10,2)   NULL,
    "TeachingMode"          VARCHAR(20)     NULL,           -- Online / Offline / Both
    "Address"               VARCHAR(300)    NULL,
    "IsVerified"            BOOLEAN         NOT NULL DEFAULT FALSE,
    "IsAvailable"           BOOLEAN         NOT NULL DEFAULT TRUE,
    "AverageRating"         DECIMAL(3,2)    NOT NULL DEFAULT 0,
    "TotalReviews"          INT             NOT NULL DEFAULT 0,
    "CreatedAt"             TIMESTAMPTZ     NOT NULL DEFAULT NOW(),
    "UpdatedAt"             TIMESTAMPTZ     NULL,
    FOREIGN KEY ("UserId") REFERENCES "AspNetUsers"("Id") ON DELETE CASCADE
);

-- Môn dạy của gia sư (many-to-many)
CREATE TABLE "TutorSubjects" (
    "TutorProfileId"    INT NOT NULL,
    "SubjectId"         INT NOT NULL,
    PRIMARY KEY ("TutorProfileId", "SubjectId"),
    FOREIGN KEY ("TutorProfileId") REFERENCES "TutorProfiles"("Id") ON DELETE CASCADE,
    FOREIGN KEY ("SubjectId")      REFERENCES "Subjects"("Id") ON DELETE CASCADE
);

-- Chứng chỉ / Bằng cấp / IELTS
CREATE TABLE "TutorCertificates" (
    "Id"            INT             NOT NULL GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    "TutorProfileId" INT            NOT NULL,
    "Title"         VARCHAR(200)    NOT NULL,   -- IELTS 8.0 / Bằng ĐH...
    "IssuedBy"      VARCHAR(200)    NULL,
    "IssuedDate"    DATE            NULL,
    "FileUrl"       VARCHAR(500)    NULL,
    "CertType"      VARCHAR(50)     NULL,       -- Degree / Certificate / IELTS / TOEIC
    "IsVerified"    BOOLEAN         NOT NULL DEFAULT FALSE,
    "AiVerifyNote"  TEXT            NULL,       -- AI kiểm duyệt ghi chú
    "CreatedAt"     TIMESTAMPTZ     NOT NULL DEFAULT NOW(),
    FOREIGN KEY ("TutorProfileId") REFERENCES "TutorProfiles"("Id") ON DELETE CASCADE
);

-- Lịch rảnh của gia sư
CREATE TABLE "TutorAvailabilities" (
    "Id"            INT     NOT NULL GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    "TutorProfileId" INT    NOT NULL,
    "DayOfWeek"     INT     NOT NULL,   -- 0=Sun ... 6=Sat
    "StartTime"     TIME    NOT NULL,
    "EndTime"       TIME    NOT NULL,
    FOREIGN KEY ("TutorProfileId") REFERENCES "TutorProfiles"("Id") ON DELETE CASCADE
);

-- Demo lesson
CREATE TABLE "DemoLessons" (
    "Id"            INT             NOT NULL GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    "TutorProfileId" INT            NOT NULL,
    "Title"         VARCHAR(200)    NOT NULL,
    "Description"   TEXT            NULL,
    "VideoUrl"      VARCHAR(500)    NULL,
    "CreatedAt"     TIMESTAMPTZ     NOT NULL DEFAULT NOW(),
    FOREIGN KEY ("TutorProfileId") REFERENCES "TutorProfiles"("Id") ON DELETE CASCADE
);

-- ============================================================
-- MODULE 3: YÊU CẦU THUÊ GIA SƯ (Job Posting)
-- ============================================================

CREATE TABLE "JobPostings" (
    "Id"                INT             NOT NULL GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    "StudentId"         VARCHAR(450)    NOT NULL,       -- UserId của học viên
    "Title"             VARCHAR(200)    NOT NULL,
    "Description"       TEXT            NULL,
    "SubjectId"         INT             NOT NULL,
    "DesiredLevel"      VARCHAR(100)    NULL,           -- VD: IELTS 7.5+
    "TeachingMode"      VARCHAR(20)     NULL,           -- Online / Offline / Both
    "Address"           VARCHAR(300)    NULL,
    "BudgetMin"         DECIMAL(10,2)   NULL,
    "BudgetMax"         DECIMAL(10,2)   NULL,
    "SessionsPerWeek"   INT             NULL,
    "SessionDuration"   INT             NULL,           -- phút
    "Deadline"          TIMESTAMPTZ     NULL,
    "Status"            VARCHAR(20)     NOT NULL DEFAULT 'Open',  -- Open/Closed/Expired
    "CreatedAt"         TIMESTAMPTZ     NOT NULL DEFAULT NOW(),
    "UpdatedAt"         TIMESTAMPTZ     NULL,
    FOREIGN KEY ("StudentId")  REFERENCES "AspNetUsers"("Id"),
    FOREIGN KEY ("SubjectId")  REFERENCES "Subjects"("Id")
);

-- ============================================================
-- MODULE 4: AI MATCHING
-- ============================================================

CREATE TABLE "MatchingResults" (
    "Id"                INT             NOT NULL GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    "JobPostingId"      INT             NULL,           -- NULL nếu gợi ý tổng quát
    "StudentId"         VARCHAR(450)    NOT NULL,
    "TutorProfileId"    INT             NOT NULL,
    "SimilarityScore"   DECIMAL(5,4)    NOT NULL,       -- 0.0 - 1.0
    "Rank"              INT             NOT NULL,
    "ModelVersion"      VARCHAR(50)     NULL,           -- sentence-transformer version
    "CreatedAt"         TIMESTAMPTZ     NOT NULL DEFAULT NOW(),
    FOREIGN KEY ("StudentId")      REFERENCES "AspNetUsers"("Id"),
    FOREIGN KEY ("TutorProfileId") REFERENCES "TutorProfiles"("Id"),
    FOREIGN KEY ("JobPostingId")   REFERENCES "JobPostings"("Id")
);

-- ============================================================
-- MODULE 5: QUY TRÌNH THUÊ (Application)
-- ============================================================

CREATE TABLE "Applications" (
    "Id"            INT             NOT NULL GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    "JobPostingId"  INT             NOT NULL,
    "TutorId"       VARCHAR(450)    NOT NULL,   -- UserId gia sư
    "CoverNote"     TEXT            NULL,
    "ProposedRate"  DECIMAL(10,2)   NULL,
    "Status"        VARCHAR(20)     NOT NULL DEFAULT 'Pending',
                                                -- Pending/Accepted/Rejected/Cancelled
    "AppliedAt"     TIMESTAMPTZ     NOT NULL DEFAULT NOW(),
    "UpdatedAt"     TIMESTAMPTZ     NULL,
    FOREIGN KEY ("JobPostingId") REFERENCES "JobPostings"("Id"),
    FOREIGN KEY ("TutorId")      REFERENCES "AspNetUsers"("Id")
);

-- ============================================================
-- MODULE 6: CHAT REALTIME (SignalR)
-- ============================================================

CREATE TABLE "Conversations" (
    "Id"        INT             NOT NULL GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    "User1Id"   VARCHAR(450)    NOT NULL,
    "User2Id"   VARCHAR(450)    NOT NULL,
    "CreatedAt" TIMESTAMPTZ     NOT NULL DEFAULT NOW(),
    FOREIGN KEY ("User1Id") REFERENCES "AspNetUsers"("Id"),
    FOREIGN KEY ("User2Id") REFERENCES "AspNetUsers"("Id")
);

CREATE TABLE "Messages" (
    "Id"                INT             NOT NULL GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    "ConversationId"    INT             NOT NULL,
    "SenderId"          VARCHAR(450)    NOT NULL,
    "Content"           TEXT            NULL,
    "FileUrl"           VARCHAR(500)    NULL,
    "FileType"          VARCHAR(20)     NULL,    -- text / image / pdf
    "IsRead"            BOOLEAN         NOT NULL DEFAULT FALSE,
    "IsFlagged"         BOOLEAN         NOT NULL DEFAULT FALSE,  -- AI phát hiện lừa đảo
    "AiFlagNote"        TEXT            NULL,
    "SentAt"            TIMESTAMPTZ     NOT NULL DEFAULT NOW(),
    FOREIGN KEY ("ConversationId") REFERENCES "Conversations"("Id") ON DELETE CASCADE,
    FOREIGN KEY ("SenderId")       REFERENCES "AspNetUsers"("Id")
);

-- ============================================================
-- MODULE 7: LỊCH HỌC (Booking / Calendar)
-- ============================================================

CREATE TABLE "Bookings" (
    "Id"                INT             NOT NULL GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    "ApplicationId"     INT             NOT NULL,
    "StudentId"         VARCHAR(450)    NOT NULL,
    "TutorId"           VARCHAR(450)    NOT NULL,
    "ScheduledStart"    TIMESTAMPTZ     NOT NULL,
    "ScheduledEnd"      TIMESTAMPTZ     NOT NULL,
    "MeetingUrl"        VARCHAR(500)    NULL,       -- link Google Meet / Zoom
    "Status"            VARCHAR(20)     NOT NULL DEFAULT 'Pending',
                                                    -- Pending/Confirmed/Cancelled/Completed
    "CheckInAt"         TIMESTAMPTZ     NULL,
    "CheckOutAt"        TIMESTAMPTZ     NULL,
    "Note"              TEXT            NULL,
    "CreatedAt"         TIMESTAMPTZ     NOT NULL DEFAULT NOW(),
    FOREIGN KEY ("ApplicationId") REFERENCES "Applications"("Id"),
    FOREIGN KEY ("StudentId")     REFERENCES "AspNetUsers"("Id"),
    FOREIGN KEY ("TutorId")       REFERENCES "AspNetUsers"("Id")
);

-- ============================================================
-- MODULE 9: THANH TOÁN
-- ============================================================

CREATE TABLE "Wallets" (
    "Id"        INT             NOT NULL GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    "UserId"    VARCHAR(450)    NOT NULL UNIQUE,
    "Balance"   DECIMAL(15,2)   NOT NULL DEFAULT 0,
    "UpdatedAt" TIMESTAMPTZ     NOT NULL DEFAULT NOW(),
    FOREIGN KEY ("UserId") REFERENCES "AspNetUsers"("Id") ON DELETE CASCADE
);

CREATE TABLE "Transactions" (
    "Id"            INT             NOT NULL GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    "WalletId"      INT             NOT NULL,
    "BookingId"     INT             NULL,
    "Amount"        DECIMAL(15,2)   NOT NULL,
    "Type"          VARCHAR(20)     NOT NULL,    -- Deposit/Withdraw/Payment/Refund
    "Gateway"       VARCHAR(30)     NULL,        -- VNPay/MoMo/Stripe/Wallet
    "GatewayRef"    VARCHAR(200)    NULL,        -- mã giao dịch từ cổng thanh toán
    "Status"        VARCHAR(20)     NOT NULL DEFAULT 'Pending',  -- Pending/Success/Failed
    "Note"          TEXT            NULL,
    "CreatedAt"     TIMESTAMPTZ     NOT NULL DEFAULT NOW(),
    FOREIGN KEY ("WalletId")  REFERENCES "Wallets"("Id"),
    FOREIGN KEY ("BookingId") REFERENCES "Bookings"("Id")
);

-- ============================================================
-- MODULE 10: ĐÁNH GIÁ (Review & Rating)
-- ============================================================

CREATE TABLE "Reviews" (
    "Id"            INT             NOT NULL GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    "BookingId"     INT             NOT NULL UNIQUE,
    "ReviewerId"    VARCHAR(450)    NOT NULL,    -- Student
    "TutorProfileId" INT            NOT NULL,
    "Rating"        SMALLINT        NOT NULL CHECK ("Rating" BETWEEN 1 AND 5),
    "Comment"       TEXT            NULL,
    "TutorReply"    TEXT            NULL,
    "IsSpam"        BOOLEAN         NOT NULL DEFAULT FALSE,
    "AiSpamNote"    TEXT            NULL,
    "CreatedAt"     TIMESTAMPTZ     NOT NULL DEFAULT NOW(),
    FOREIGN KEY ("BookingId")       REFERENCES "Bookings"("Id"),
    FOREIGN KEY ("ReviewerId")      REFERENCES "AspNetUsers"("Id"),
    FOREIGN KEY ("TutorProfileId")  REFERENCES "TutorProfiles"("Id")
);

-- ============================================================
-- MODULE 11: THÔNG BÁO (Notification)
-- ============================================================

CREATE TABLE "Notifications" (
    "Id"            INT             NOT NULL GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    "UserId"        VARCHAR(450)    NOT NULL,
    "Title"         VARCHAR(200)    NOT NULL,
    "Body"          TEXT            NULL,
    "Type"          VARCHAR(50)     NULL,    -- NewJob/NewApply/Booking/Chat/Payment/Rating
    "ReferenceId"   INT             NULL,    -- Id của entity liên quan
    "IsRead"        BOOLEAN         NOT NULL DEFAULT FALSE,
    "CreatedAt"     TIMESTAMPTZ     NOT NULL DEFAULT NOW(),
    FOREIGN KEY ("UserId") REFERENCES "AspNetUsers"("Id") ON DELETE CASCADE
);

-- ============================================================
-- MODULE 8 & 12: ADMIN + AI LOGGING
-- ============================================================

-- Báo cáo / Khiếu nại
CREATE TABLE "Reports" (
    "Id"            INT             NOT NULL GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    "ReporterId"    VARCHAR(450)    NOT NULL,
    "TargetUserId"  VARCHAR(450)    NULL,
    "TargetType"    VARCHAR(50)     NULL,    -- User/Review/Message/JobPosting
    "TargetId"      INT             NULL,
    "Reason"        TEXT            NOT NULL,
    "Status"        VARCHAR(20)     NOT NULL DEFAULT 'Pending',  -- Pending/Resolved/Dismissed
    "AdminNote"     TEXT            NULL,
    "CreatedAt"     TIMESTAMPTZ     NOT NULL DEFAULT NOW(),
    FOREIGN KEY ("ReporterId")   REFERENCES "AspNetUsers"("Id"),
    FOREIGN KEY ("TargetUserId") REFERENCES "AspNetUsers"("Id")
);

-- AI Activity Log
CREATE TABLE "AiLogs" (
    "Id"            INT             NOT NULL GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    "UserId"        VARCHAR(450)    NULL,
    "Action"        VARCHAR(100)    NOT NULL,   -- Matching/CertVerify/SpamDetect/Chatbot/Quiz
    "InputData"     TEXT            NULL,
    "OutputData"    TEXT            NULL,
    "ModelUsed"     VARCHAR(100)    NULL,
    "DurationMs"    INT             NULL,
    "CreatedAt"     TIMESTAMPTZ     NOT NULL DEFAULT NOW(),
    FOREIGN KEY ("UserId") REFERENCES "AspNetUsers"("Id")
);

-- ============================================================
-- INDEXES tối ưu query phổ biến
-- ============================================================

CREATE INDEX "IX_AspNetUsers_Email"          ON "AspNetUsers"("NormalizedEmail");
CREATE INDEX "IX_TutorProfiles_UserId"       ON "TutorProfiles"("UserId");
CREATE INDEX "IX_TutorProfiles_IsAvailable"  ON "TutorProfiles"("IsAvailable", "IsVerified");
CREATE INDEX "IX_JobPostings_StudentId"      ON "JobPostings"("StudentId");
CREATE INDEX "IX_JobPostings_Status"         ON "JobPostings"("Status", "Deadline");
CREATE INDEX "IX_Applications_JobPostingId"  ON "Applications"("JobPostingId");
CREATE INDEX "IX_Applications_TutorId"       ON "Applications"("TutorId");
CREATE INDEX "IX_Bookings_StudentId"         ON "Bookings"("StudentId");
CREATE INDEX "IX_Bookings_TutorId"           ON "Bookings"("TutorId");
CREATE INDEX "IX_Messages_ConversationId"    ON "Messages"("ConversationId", "SentAt");
CREATE INDEX "IX_Notifications_UserId"       ON "Notifications"("UserId", "IsRead");
CREATE INDEX "IX_MatchingResults_StudentId"  ON "MatchingResults"("StudentId");

-- ============================================================
-- SEED DATA
-- ============================================================

-- Roles mặc định
INSERT INTO "AspNetRoles" ("Id", "Name", "NormalizedName", "ConcurrencyStamp") VALUES
    (gen_random_uuid()::TEXT, 'Admin',   'ADMIN',   gen_random_uuid()::TEXT),
    (gen_random_uuid()::TEXT, 'Tutor',   'TUTOR',   gen_random_uuid()::TEXT),
    (gen_random_uuid()::TEXT, 'Student', 'STUDENT', gen_random_uuid()::TEXT),
    (gen_random_uuid()::TEXT, 'Guest',   'GUEST',   gen_random_uuid()::TEXT);

-- Môn học mẫu
INSERT INTO "Subjects" ("Name", "Category") VALUES
    ('Toán',            'THPT'),
    ('Vật lý',          'THPT'),
    ('Hóa học',         'THPT'),
    ('Tiếng Anh',       'Ngoại ngữ'),
    ('IELTS',           'Ngoại ngữ'),
    ('TOEIC',           'Ngoại ngữ'),
    ('Lập trình C#',    'Công nghệ'),
    ('Lập trình Python','Công nghệ'),
    ('Cơ sở dữ liệu',   'Công nghệ'),
    ('Văn học',         'THPT'),
    ('Lịch sử',         'THPT'),
    ('Địa lý',          'THPT');

-- ============================================================

\echo 'StudyMate PostgreSQL schema created successfully.';
