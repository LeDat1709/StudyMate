# Plan Setup — StudyMate

> **Phiên bản:** 1.0  
> **Ngày:** 2026-07-10  
> **Mục đích:** Xác định kiến trúc tổng thể, cấu trúc thư mục, tech stack, dependency giữa các module  
> trước khi bắt đầu chia task và tạo prompt.xml.

---

## 1. Trạng Thái Hiện Tại (Đã Hoàn Thành)

```
✅ Database schema thiết kế xong (SQL Server + PostgreSQL)
✅ ASP.NET Identity cấu hình xong (ApplicationUser, ApplicationDbContext)
✅ Migration InitIdentity đã apply lên SQL Server
✅ Seed Roles: Admin / Tutor / Student / Guest
✅ appsettings.json đã có ConnectionString, Email, OTP config
✅ Packages đã cài: Identity.EFCore, EFCore.SqlServer, EFCore.Tools
```

---

## 2. Cấu Trúc Thư Mục Chuẩn

Dưới đây là cấu trúc **mục tiêu** toàn bộ project sau khi hoàn thành.  
Mỗi thư mục mới được tạo ra đúng khi module tương ứng bắt đầu triển khai.

```
StudyMate/
│
├── Controllers/                        ← HTTP request handler
│   ├── HomeController.cs               ✅ có sẵn
│   ├── AccountController.cs            ← M1: Auth
│   ├── TutorProfileController.cs       ← M2: Tutor Profile
│   ├── JobPostingController.cs         ← M3: Job Posting
│   ├── ApplicationController.cs        ← M5: Apply flow
│   ├── ChatController.cs               ← M6: Chat
│   ├── BookingController.cs            ← M7: Booking
│   ├── NotificationController.cs       ← M11: Notification
│   └── Admin/
│       └── AdminController.cs          ← M8: Admin
│
├── Models/                             ← EF Core entities (DB mapping)
│   ├── ApplicationUser.cs              ✅ có sẵn
│   ├── OtpCode.cs                      ✅ có sẵn
│   ├── ErrorViewModel.cs               ✅ có sẵn
│   ├── Subject.cs                      ← M2
│   ├── TutorProfile.cs                 ← M2
│   ├── TutorSubject.cs                 ← M2
│   ├── TutorCertificate.cs             ← M2
│   ├── TutorAvailability.cs            ← M2
│   ├── DemoLesson.cs                   ← M2
│   ├── JobPosting.cs                   ← M3
│   ├── MatchingResult.cs               ← M4
│   ├── Application.cs                  ← M5
│   ├── Conversation.cs                 ← M6
│   ├── Message.cs                      ← M6
│   ├── Booking.cs                      ← M7
│   ├── Wallet.cs                       ← M9
│   ├── Transaction.cs                  ← M9
│   ├── Review.cs                       ← M10
│   ├── Notification.cs                 ← M11
│   ├── Report.cs                       ← M8
│   └── AiLog.cs                        ← M8/M12
│
├── ViewModels/                         ← DTO dùng cho Razor View (không map trực tiếp DB)
│   ├── Account/
│   │   ├── RegisterViewModel.cs        ← M1
│   │   ├── LoginViewModel.cs           ← M1
│   │   ├── ForgotPasswordViewModel.cs  ← M1
│   │   ├── VerifyOtpViewModel.cs       ← M1
│   │   ├── ResetPasswordViewModel.cs   ← M1
│   │   └── ProfileViewModel.cs        ← M1
│   ├── TutorProfile/
│   │   └── TutorProfileViewModel.cs   ← M2
│   ├── JobPosting/
│   │   └── JobPostingViewModel.cs     ← M3
│   └── ...
│
├── Services/                           ← Business logic (interface + implementation)
│   ├── Interfaces/
│   │   ├── IEmailService.cs            ← M1
│   │   ├── IOtpService.cs              ← M1
│   │   ├── IFileStorageService.cs      ← M2
│   │   ├── IMatchingService.cs         ← M4
│   │   └── INotificationService.cs    ← M11
│   └── Implementations/
│       ├── EmailService.cs             ← M1
│       ├── OtpService.cs               ← M1
│       ├── FileStorageService.cs       ← M2
│       ├── MatchingService.cs          ← M4 (gọi Python FastAPI)
│       └── NotificationService.cs     ← M11
│
├── Hubs/                               ← SignalR Hubs
│   ├── ChatHub.cs                      ← M6
│   └── NotificationHub.cs             ← M11
│
├── Data/                               ← DbContext + Seed data
│   └── ApplicationDbContext.cs         ✅ có sẵn
│
├── Migrations/                         ← EF Core migrations (auto-generated)
│   └── 20260710_InitIdentity.*         ✅ có sẵn
│
├── Views/                              ← Razor Views
│   ├── Shared/
│   │   ├── _Layout.cshtml              ✅ có sẵn (cần update Bootstrap 5)
│   │   ├── _ValidationScriptsPartial  ✅ có sẵn
│   │   └── _Navbar.cshtml             ← thêm sau
│   ├── Home/
│   │   ├── Index.cshtml                ✅ có sẵn (cần redesign)
│   │   └── Privacy.cshtml             ✅ có sẵn
│   ├── Account/                        ← M1
│   │   ├── Register.cshtml
│   │   ├── Login.cshtml
│   │   ├── VerifyEmail.cshtml
│   │   ├── ForgotPassword.cshtml
│   │   ├── VerifyResetOtp.cshtml
│   │   ├── ResetPassword.cshtml
│   │   └── Profile.cshtml
│   ├── TutorProfile/                   ← M2
│   ├── JobPosting/                     ← M3
│   ├── Application/                    ← M5
│   ├── Chat/                           ← M6
│   ├── Booking/                        ← M7
│   └── Admin/                          ← M8
│
├── wwwroot/                            ← Static files
│   ├── css/
│   │   └── site.css                    ✅ có sẵn
│   ├── js/
│   │   └── site.js                     ✅ có sẵn
│   ├── lib/
│   │   ├── bootstrap/                  ✅ có sẵn (Bootstrap 5)
│   │   ├── jquery/                     ✅ có sẵn
│   │   ├── jquery-validation/          ✅ có sẵn
│   │   └── jquery-validation-unobtrusive/ ✅ có sẵn
│   └── uploads/                        ← thư mục lưu file upload (tạo khi cần)
│       ├── avatars/
│       ├── certificates/
│       └── chat/
│
├── Docs/                               ← Tài liệu dự án
│   ├── AI_Agent_Code_Process.md        ✅
│   ├── PlanSetup.md                    ✅ file này
│   ├── FRS/                            ✅ 12 file FRS
│   ├── Prompts/                        ← prompt.xml cho từng task
│   └── Reports/                        ← báo cáo sau mỗi task
│
├── appsettings.json                    ✅ có sẵn
├── appsettings.Development.json        ✅ có sẵn
└── Program.cs                          ✅ có sẵn
```

---

## 3. Tech Stack Chi Tiết

### Backend
| Thành phần | Công nghệ | Phiên bản | Ghi chú |
|---|---|---|---|
| Framework | ASP.NET Core MVC | .NET 10 | |
| ORM | Entity Framework Core | 10.0.0 | ✅ đã cài |
| Auth | ASP.NET Identity | 10.0.0 | ✅ đã cài |
| DB Driver | EFCore.SqlServer | 10.0.0 | ✅ đã cài |
| Migration tool | EFCore.Tools | 10.0.0 | ✅ đã cài |
| Realtime | SignalR | built-in .NET 10 | thêm khi M6 |
| Email | MailKit hoặc SMTP built-in | - | thêm khi M1-T5 |
| File storage | Local wwwroot/uploads | - | dev; cloud sau |
| Background jobs | IHostedService / Hangfire | - | thêm khi M7/M11 |

### Frontend
| Thành phần | Công nghệ | Ghi chú |
|---|---|---|
| UI Framework | Bootstrap 5 | ✅ có sẵn trong wwwroot/lib |
| DOM/AJAX | jQuery | ✅ có sẵn |
| Form validation | jQuery Validation + Unobtrusive | ✅ có sẵn |
| Calendar UI | FullCalendar.js | thêm khi M7 |
| Realtime client | SignalR JS client | thêm khi M6 |

### Database
| Môi trường | Database | Connection |
|---|---|---|
| Development | SQL Server Express (`DAT\SQLEXPRESS`) | Windows Auth |
| Production | PostgreSQL (Render) | connection string env var |

### AI Service (riêng biệt)
| Thành phần | Công nghệ | Ghi chú |
|---|---|---|
| API Gateway | Python FastAPI | project riêng |
| Matching | Sentence Transformer | M4 |
| Chatbot/RAG | LangChain + ChromaDB + OpenAI/Ollama | M12 |
| Spam detection | Text classifier | M6/M10 |

---

## 4. Packages Cần Cài Theo Module

Chỉ cài khi bắt đầu module tương ứng — không cài trước.

| Package | Module | Lệnh |
|---|---|---|
| `MailKit` | M1-T5 (Email Service) | `dotnet add package MailKit` |
| `Microsoft.AspNetCore.SignalR` | M6 (Chat) | built-in, chỉ cần thêm client JS |
| `Hangfire` hoặc dùng `IHostedService` | M7/M11 (Reminder) | tùy chọn |
| `SixLabors.ImageSharp` | M2 (resize ảnh) | `dotnet add package SixLabors.ImageSharp` |
| `Npgsql.EntityFrameworkCore.PostgreSQL` | Deploy lên Render | `dotnet add package Npgsql.EntityFrameworkCore.PostgreSQL` |

---

## 5. Dependency Map Giữa Các Module

```
M1 (Auth + Identity)          ← nền tảng, phải xong trước tất cả
  │
  ├──▶ M2 (Tutor Profile)     ← cần ApplicationUser, Subjects
  │
  ├──▶ M3 (Job Posting)       ← cần ApplicationUser, Subjects
  │         │
  │         ├──▶ M4 (AI Matching)     ← cần TutorProfile + JobPosting
  │         │
  │         └──▶ M5 (Application)     ← cần JobPosting + TutorProfile
  │                   │
  │                   ├──▶ M6 (Chat)      ← cần Application.Accepted
  │                   │
  │                   └──▶ M7 (Booking)   ← cần Application.Accepted
  │                               │
  │                               ├──▶ M9 (Payment)    ← cần Booking.Completed
  │                               └──▶ M10 (Review)    ← cần Booking.Completed
  │
  └──▶ M11 (Notification)     ← cần tất cả module trên (trigger events)
  └──▶ M8  (Admin)            ← cần tất cả module trên (read-only + moderation)
  └──▶ M12 (AI Assistant)     ← cần M2, M3, M4, M7
```

**Quy tắc triển khai từ dependency map:**
- Không bắt đầu M2 khi M1 chưa xong
- Không bắt đầu M5 khi M3 hoặc M2 chưa xong
- M8 và M11 có thể làm song song sau khi M1–M5 xong
- M12 làm cuối cùng

---

## 6. Thứ Tự Triển Khai (Sprint Plan)

| Sprint | Module | Nội dung chính | Phụ thuộc |
|---|---|---|---|
| 1 | M1 | Auth, Identity, OTP, Profile | — |
| 2 | M2 | Tutor Profile, Certificates, Availability | M1 |
| 3 | M3 | Job Posting, Search, Filter | M1, M2 |
| 4 | M5 | Application Flow (Apply/Accept/Reject) | M2, M3 |
| 5 | M6 | Chat Realtime (SignalR) | M5 |
| 5 | M7 | Booking / Calendar | M5 |
| 6 | M4 | AI Matching (Python FastAPI) | M2, M3 |
| 6 | M8 | Admin Dashboard | M1–M5 |
| 7 | M9 | Payment (VNPay/MoMo) | M7 |
| 7 | M10 | Review & Rating | M7 |
| 8 | M11 | Notification (SignalR) | M1–M10 |
| 9 | M12 | AI Learning Assistant | M2, M3, M7 |

---

## 7. Quy Ước Code (Coding Conventions)

### Naming
| Loại | Convention | Ví dụ |
|---|---|---|
| Class | PascalCase | `ApplicationUser`, `TutorProfile` |
| Interface | `I` + PascalCase | `IEmailService` |
| Method | PascalCase | `GetTutorByIdAsync` |
| Variable/param | camelCase | `tutorProfile`, `userId` |
| Private field | `_` + camelCase | `_userManager`, `_context` |
| Constant | UPPER_SNAKE_CASE | `MAX_OTP_ATTEMPTS` |
| Route | kebab-case | `/tutor-profile/edit` |

### File tổ chức
- 1 class = 1 file
- Tên file = tên class
- Namespace = `StudyMate.[Folder]`

### Async
- Tất cả database call phải dùng `async/await`
- Method async phải có suffix `Async`: `CreateUserAsync()`

### Dependency Injection
- Đăng ký service trong `Program.cs`
- Inject qua constructor, không dùng service locator

### Comment
- Public method phải có XML doc comment
- Logic phức tạp phải có inline comment tiếng Anh

---

## 8. Cấu Hình Môi Trường

### Development (local)
```json
// appsettings.Development.json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=DAT\\SQLEXPRESS;Database=StudyMate;Trusted_Connection=True;TrustServerCertificate=True;"
  },
  "Email": {
    "SmtpHost": "smtp.gmail.com",
    "SmtpPort": 587,
    "SenderEmail": "your-dev-email@gmail.com",
    "SenderName": "StudyMate Dev",
    "Password": "your-app-password"
  }
}
```

### Production (Render + PostgreSQL)
```
Dùng Environment Variables trên Render dashboard:
- ConnectionStrings__DefaultConnection = Host=...;Database=studymate;...
- Email__Password = <secret>
- ASPNETCORE_ENVIRONMENT = Production
```

> **Không commit credential vào git.**  
> Dùng `dotnet user-secrets` cho dev local nếu cần bảo mật cao hơn.

---

## 9. Git Branch Convention

```
main                    ← production, chỉ merge qua PR
develop                 ← integration branch

feat/m1-auth            ← Module 1
feat/m2-tutor-profile   ← Module 2
feat/m3-job-posting     ← Module 3
feat/m4-ai-matching     ← Module 4
feat/m5-application     ← Module 5
feat/m6-chat            ← Module 6
feat/m7-booking         ← Module 7
feat/m8-admin           ← Module 8
feat/m9-payment         ← Module 9
feat/m10-review         ← Module 10
feat/m11-notification   ← Module 11
feat/m12-ai-assistant   ← Module 12

fix/[mô tả ngắn]        ← hotfix
chore/[mô tả ngắn]      ← setup, config, docs
```

**Commit message convention:**
```
feat(m1): add Register page with OTP email
fix(m2): correct avatar upload path
chore: update appsettings connection string
docs: add FRS Module 3
refactor(m1): extract OtpService to separate class
```

---

## 10. Checklist Trước Khi Bắt Đầu Task Mới

Trước khi tạo `prompt.xml` cho bất kỳ task nào, kiểm tra:

- [ ] FRS của module đó đã được viết và review
- [ ] Module dependency đã hoàn thành (theo mục 5)
- [ ] Branch `feat/m[n]-[tên]` đã được tạo từ `develop`
- [ ] `appsettings` đã có config cần thiết cho task đó
- [ ] Hiểu rõ file nào sẽ bị ảnh hưởng bởi task

---

*Plan Setup v1.0 — StudyMate — 2026-07-10*
