# Break Task — Module 2: Quản Lý Hồ Sơ Gia Sư

> **Module:** M2  
> **FRS Reference:** `Docs/FRS/FRS_M2_TutorProfile.md`  
> **Plan Reference:** `Docs/PlanSetup.md`  
> **Phụ thuộc:** M1 (Auth) — đã Done  
> **Branch gợi ý:** `feat/m2-tutor-profile`  
> **Trạng thái đã có sẵn (từ M1):**
> - ✅ `ApplicationUser` (FullName, AvatarUrl, Address, …)
> - ✅ Identity + Roles (Tutor / Student / Admin / Guest)
> - ✅ `TutorCertificate` model **tạm** gắn `UserId` (AspNetUsers) — **M2 phải migrate → `TutorProfileId`**
> - ✅ Upload chứng chỉ tại `/Account/Certificates` (M1-T9)
> - ✅ Upload avatar account tại `/Account/Profile`
> - ✅ Schema SQL tham chiếu: `Database/StudyMate_Schema.sql` (Subjects, TutorProfiles, TutorSubjects, TutorCertificates, TutorAvailabilities, DemoLessons)

---

## Dependency giữa các Task

```
M2-T1 (Models core + Seed Subjects + Migration)
  │
  ├──▶ M2-T2 (Certificate FK migrate + Availability/DemoLesson models)
  │         │
  │         └──▶ M2-T3 (FileStorageService)     ← dùng chung upload file
  │                   │
  │                   ├──▶ M2-T4 (Create/Edit Tutor Profile form)
  │                   │         │
  │                   │         ├──▶ M2-T5 (Video intro URL / MP4)
  │                   │         ├──▶ M2-T6 (Quản lý môn dạy multi-select)
  │                   │         ├──▶ M2-T7 (Certificates gắn TutorProfile)
  │                   │         ├──▶ M2-T8 (Lịch rảnh weekly schedule)
  │                   │         └──▶ M2-T9 (Demo Lesson CRUD)
  │                   │
  │                   └──▶ (T5–T9 cần profile đã tồn tại để test)
  │
  └──▶ M2-T10 (Public profile)     ← cần data profile + subjects (+ optional cert/avail/demo)
            │
            └──▶ M2-T11 (Search & filter)  ← cần public profile + IsVerified filter

M2-T12 (AI kiểm duyệt — optional / stub)  ← sau T7; có thể defer sang M8/M12
```

**Thứ tự bắt buộc:**
`T1 → T2 → T3 → T4`  
`T4 → T5, T6, T7, T8, T9` (có thể song song sau T4 nếu không đụng chung file lớn)  
`T4 + T6 → T10 → T11`  
`T7 → T12` (nếu làm AI)

**Không làm song song:** T1/T2 (cùng Migration/DbContext).

---

## Danh sách Task

| Task ID | Tên Task | Phụ thuộc | Ước tính | UC liên quan |
|---|---|---|---|---|
| M2-T1 | Models Subject + TutorProfile + TutorSubject, Seed, Migration | — | 60 phút | UC-M2-01, 05 |
| M2-T2 | Models Availability + DemoLesson + migrate Certificate → TutorProfileId | M2-T1 | 60 phút | UC-M2-06, 07, 08 |
| M2-T3 | FileStorageService (upload local + optional ImageSharp resize) | M2-T2 | 45 phút | UC-M2-03, 04, 06 |
| M2-T4 | Create / Edit hồ sơ gia sư (section cơ bản) | M2-T3 | 90 phút | UC-M2-01, 02 |
| M2-T5 | Video giới thiệu (YouTube/Vimeo URL + MP4) | M2-T4 | 60 phút | UC-M2-04 |
| M2-T6 | Quản lý môn dạy (multi-select TutorSubjects) | M2-T4 | 45 phút | UC-M2-05 |
| M2-T7 | Chứng chỉ gắn TutorProfile (refactor M1-T9) | M2-T4 | 60 phút | UC-M2-06 |
| M2-T8 | Quản lý lịch rảnh (weekly schedule) | M2-T4 | 90 phút | UC-M2-07 |
| M2-T9 | Demo Lesson CRUD | M2-T4 | 45 phút | UC-M2-08 |
| M2-T10 | Trang hồ sơ công khai | M2-T6 (nên có T7–T9) | 60 phút | UC-M2-09 |
| M2-T11 | Tìm kiếm & lọc gia sư | M2-T10 | 90 phút | UC-M2-10 |
| M2-T12 | AI kiểm duyệt ảnh/chứng chỉ (stub hoặc Python API) | M2-T7 | 60 phút | UC-M2-03, 06 |

---

## Chi tiết từng Task

---

### M2-T1 — Models Subject + TutorProfile + TutorSubject + Seed + Migration

**Mục tiêu:** Tạo entity cốt lõi hồ sơ gia sư, seed danh sách môn, migration DB.

**Scope:**
- Tạo `Models/Subject.cs`
- Tạo `Models/TutorProfile.cs` (map đúng `StudyMate_Schema.sql`)
- Tạo `Models/TutorSubject.cs` (composite key `TutorProfileId` + `SubjectId`)
- Cập nhật `ApplicationDbContext`: DbSet + Fluent API (unique `UserId`, indexes)
- Seed `Subjects` (dùng data mẫu trong schema: Toán, IELTS, …) khi startup **hoặc** migration seed
- Tạo EF Migration (ví dụ `AddTutorProfileCore`)

**Out of Scope:**
- Không tạo Controller/View
- Không đụng `TutorCertificate` FK (để T2)
- Không tạo Availability / DemoLesson (để T2)
- Không FileStorage

**Output cần có:**
- `StudyMate/Models/Subject.cs`
- `StudyMate/Models/TutorProfile.cs`
- `StudyMate/Models/TutorSubject.cs`
- `StudyMate/Data/ApplicationDbContext.cs` — DbSet + config
- Migration mới + snapshot
- Seed Subjects (file seed hoặc trong `Program.cs` / `Data/SeedData.cs`)

**Business Rules / Field map (`TutorProfiles`):**

| Field | Type | Note |
|---|---|---|
| UserId | string | UNIQUE, FK AspNetUsers |
| Headline | string? | max 200 |
| Bio | string? | |
| VideoIntroUrl | string? | max 500 |
| YearsOfExperience | int? | ≥ 0 |
| EducationLevel | string? | Cử nhân / Thạc sĩ / Tiến sĩ / Khác |
| HourlyRate | decimal? | > 0 khi publish |
| TeachingMode | string? | Online / Offline / Both |
| Address | string? | bắt buộc nếu Offline (validate ở T4) |
| IsVerified | bool | default false (chờ Admin) |
| IsAvailable | bool | default true |
| AverageRating | decimal | default 0 |
| TotalReviews | int | default 0 |
| CreatedAt / UpdatedAt | DateTime | |

**Acceptance Criteria:**
- [ ] Migration apply được, bảng `Subjects`, `TutorProfiles`, `TutorSubjects` tồn tại
- [ ] `UserId` unique trên `TutorProfiles`
- [ ] Seed ≥ 8 subjects (theo schema mẫu)
- [ ] `dotnet build` pass
- [ ] Không sửa logic AccountController

**Test Plan:**
1. `dotnet ef migrations add AddTutorProfileCore` → generate OK  
2. `dotnet ef database update` → success  
3. Query `Subjects` → có dữ liệu seed  
4. `dotnet build` → 0 error  

**Ước tính:** 60 phút

---

### M2-T2 — Availability + DemoLesson models + migrate Certificate → TutorProfileId

**Mục tiêu:** Hoàn thiện model còn lại của M2; **sửa debt M1-T9**: chuyển `TutorCertificates.UserId` → `TutorProfileId`.

**Scope:**
- Tạo `Models/TutorAvailability.cs` (`DayOfWeek` 0–6, `StartTime`, `EndTime`)
- Tạo `Models/DemoLesson.cs` (`Title`, `Description`, `VideoUrl`, `CreatedAt`)
- Sửa `Models/TutorCertificate.cs`:
  - Thêm `TutorProfileId`
  - Bỏ (hoặc deprecate) `UserId` sau khi migrate data
- Fluent API + Migration:
  - Thêm cột `TutorProfileId` (nullable tạm)
  - Data migration SQL: map `UserId` → `TutorProfiles.Id` (user đã có profile) **hoặc** ghi rõ: chỉ migrate khi đã có profile; cert của user chưa có profile → giữ/ghi log
  - Drop FK `UserId`, set `TutorProfileId` NOT NULL sau khi data sạch
- Cập nhật `AccountController` Certificates **tạm** để build không vỡ:
  - Option A (khuyến nghị): chỉ sửa model + migration; nếu AccountController còn compile lỗi → fix minimal (filter theo User → profile) **hoặc** comment/redirect sang route M2-T7
  - Không redesign UI Certificates ở task này

**Out of Scope:**
- Không làm UI Availability / DemoLesson
- Không làm Admin duyệt chứng chỉ
- Không AI verify

**Output cần có:**
- `Models/TutorAvailability.cs`
- `Models/DemoLesson.cs`
- `Models/TutorCertificate.cs` (đã đổi FK)
- `ApplicationDbContext.cs` cập nhật
- Migration (ví dụ `AddTutorProfileRelatedAndMigrateCertificates`)
- Fix compile tối thiểu cho code M1-T9 nếu vỡ

**Business Rules:**
- 1 `TutorProfile` : N certificates / availabilities / demo lessons
- Certificate `IsVerified` default false
- DayOfWeek: 0 = Sunday … 6 = Saturday (chuẩn .NET) **hoặc** document nếu dùng 1–7; chọn 1 convention và ghi rõ trong code

**Acceptance Criteria:**
- [ ] Bảng `TutorAvailabilities`, `DemoLessons` tồn tại
- [ ] `TutorCertificates` có FK `TutorProfileId` (không còn phụ thuộc chỉ `UserId`)
- [ ] Project build pass
- [ ] Migration có chiến lược data (script hoặc note trong report nếu dev DB trống)

**Test Plan:**
1. `dotnet build` → pass  
2. Apply migration  
3. Kiểm tra schema: `TutorCertificates.TutorProfileId` + FK  
4. (Nếu có data M1) verify cert map đúng profile sau khi tạo profile test  

**Ước tính:** 60 phút

---

### M2-T3 — FileStorageService

**Mục tiêu:** Service upload file dùng chung cho avatar hồ sơ, video, chứng chỉ (tránh lặp logic path/validate).

**Scope:**
- Tạo `Services/Interfaces/IFileStorageService.cs`
- Tạo `Services/Implementations/FileStorageService.cs`
- Methods gợi ý:
  - `Task<string> SaveAsync(IFormFile file, string subFolder, string[] allowedExtensions, long maxBytes)`
  - `Task DeleteIfExistsAsync(string? relativeUrl)` (optional nhưng nên có — xử lý debt file rác M1)
- Lưu dưới `wwwroot/uploads/{subFolder}/`
- Tên file: `{guid}_{safeOriginalName}` hoặc `{userId}_{timestamp}{ext}`
- Package `SixLabors.ImageSharp` (PlanSetup) + method optional:
  - `Task<string> SaveImageResizedAsync(..., int width, int height)` → resize 400×400 cho avatar hồ sơ
- Đăng ký DI Scoped trong `Program.cs`

**Out of Scope:**
- Không Azure Blob / S3
- Không virus scan
- Không refactor toàn bộ M1 avatar trừ khi chỉ 1–2 dòng để dùng service (optional)

**Output cần có:**
- `Services/Interfaces/IFileStorageService.cs`
- `Services/Implementations/FileStorageService.cs`
- `Program.cs` — DI
- Package ImageSharp nếu dùng resize

**Acceptance Criteria:**
- [ ] Inject được `IFileStorageService`
- [ ] Save file hợp lệ → trả relative URL (vd `/uploads/tutor-videos/...`)
- [ ] Sai extension / quá size → throw hoặc result rõ (document convention)
- [ ] `dotnet build` pass
- [ ] Không hardcode absolute path máy local

**Test Plan:**
1. `dotnet build`  
2. Unit/manual: gọi Save với file fake trong test action tạm (xóa trước commit)  
3. Kiểm tra file xuất hiện trong `wwwroot/uploads/...`  

**Ước tính:** 45 phút

---

### M2-T4 — Create / Edit hồ sơ gia sư (section cơ bản)

**Mục tiêu:** Tutor tạo hồ sơ lần đầu và chỉnh sửa các field cốt lõi.

**Scope:**
- Tạo `Controllers/TutorProfileController.cs` với `[Authorize(Roles = "Tutor")]`
- Actions:
  - `Create` GET/POST — chỉ khi user **chưa** có `TutorProfile`
  - `Edit` GET/POST — chỉ **chủ sở hữu**
  - `MyProfile` GET — dashboard hồ sơ (redirect Create nếu chưa có)
- ViewModels: `ViewModels/TutorProfile/TutorProfileFormViewModel.cs`
- Views: `Views/TutorProfile/Create.cshtml`, `Edit.cshtml`, `MyProfile.cshtml` (hoặc gộp Create/Edit 1 partial)
- Fields form (theo UC-M2-01):
  - Headline, Bio (≥ 100 ký tự), YearsOfExperience, EducationLevel, HourlyRate, TeachingMode, Address, IsAvailable
  - Avatar: **hiển thị** từ `ApplicationUser.AvatarUrl` (link sang Account/Profile nếu chưa có ảnh); không bắt buộc upload mới ở task này (UC-M2-03 có thể reuse M1)
- Lưu `IsVerified = false` khi Create
- Navbar: link “Hồ sơ gia sư” cho role Tutor → `MyProfile`

**Out of Scope:**
- Không multi-select subjects (T6)
- Không video (T5)
- Không certificates / availability / demo
- Không trang public
- Không Admin duyệt

**Output cần có:**
- `Controllers/TutorProfileController.cs`
- `ViewModels/TutorProfile/TutorProfileFormViewModel.cs`
- `Views/TutorProfile/Create.cshtml`, `Edit.cshtml`, `MyProfile.cshtml` (+ optional `_Form.cshtml`)
- Cập nhật `_Layout.cshtml` (link Tutor)

**Business Rules:**
- Mỗi Tutor **đúng 1** TutorProfile (`UserId` unique)
- Create lần 2 → redirect Edit
- Offline (`TeachingMode = Offline` hoặc `Both`) → Address required
- HourlyRate > 0; YearsOfExperience ≥ 0
- Chỉ owner sửa được (403 nếu Tutor khác)

**Acceptance Criteria:**
- [ ] Create thành công → record `TutorProfiles`, message “Hồ sơ đang chờ xét duyệt”
- [ ] Edit cập nhật field + `UpdatedAt`
- [ ] Student / Guest không vào Create (403 hoặc redirect)
- [ ] Validation inline hoạt động
- [ ] Build pass

**Test Plan:**
1. Login Tutor chưa có profile → Create → lưu  
2. Reload MyProfile → data đúng, status Chờ duyệt  
3. Edit Headline → DB cập nhật  
4. TeachingMode Offline không Address → lỗi  
5. Login Student → `/TutorProfile/Create` → 403  
6. `dotnet build`  

**Ước tính:** 90 phút

---

### M2-T5 — Video giới thiệu (URL + MP4)

**Mục tiêu:** Tutor gắn 1 video intro: YouTube/Vimeo URL hoặc upload MP4 ≤ 50MB.

**Scope:**
- Thêm section Video trên `Edit` / `MyProfile` (hoặc action riêng `UpdateVideo`)
- Hỗ trợ:
  - URL YouTube / Vimeo → lưu `VideoIntroUrl`
  - Upload MP4 ≤ 50MB qua `IFileStorageService` → subfolder `tutor-videos`
- Chỉ **1** video: upload mới thay thế URL/file cũ; xóa file cũ nếu local
- Validate URL format cơ bản (host youtube/vimeo hoặc path `/uploads/`)

**Out of Scope:**
- Không transcoding / thumbnail
- Không stream CDN
- Không AI

**Output cần có:**
- Cập nhật `TutorProfileController` + View Edit/MyProfile
- ViewModel field `VideoIntroUrl` / `VideoFile`
- Dùng `IFileStorageService`

**Acceptance Criteria:**
- [ ] Lưu YouTube URL → hiển thị embed (iframe) trên MyProfile
- [ ] Upload MP4 hợp lệ → player HTML5
- [ ] File > 50MB / sai định dạng → lỗi
- [ ] Video mới thay video cũ
- [ ] Build pass

**Test Plan:**
1. Dán URL YouTube hợp lệ → save → embed  
2. Upload MP4 nhỏ → player  
3. Upload .exe / > 50MB → lỗi  
4. Upload lần 2 → chỉ còn 1 video  

**Ước tính:** 60 phút

---

### M2-T6 — Quản lý môn dạy (multi-select)

**Mục tiêu:** Tutor chọn/xóa môn dạy từ bảng `Subjects` (1–10 môn).

**Scope:**
- UI checkbox/tag list trên Create (nếu còn) và Edit/MyProfile
- Actions: lưu `TutorSubjects` (replace set hoặc add/remove rõ ràng)
- Load danh sách Subjects từ DB (đã seed T1)
- Validation: min 1, max 10

**Out of Scope:**
- Không Admin CRUD Subjects (ghi note → M8)
- Không search subject phức tạp (list ≤ ~20 item OK)

**Output cần có:**
- Cập nhật ViewModel: `List<int> SelectedSubjectIds` + `IEnumerable<Subject>` display
- Controller logic sync `TutorSubjects`
- View partial multi-select

**Acceptance Criteria:**
- [ ] Chọn ≥ 1 môn → lưu `TutorSubjects` đúng
- [ ] Bỏ hết môn → lỗi “Chọn tối thiểu 1 môn”
- [ ] > 10 môn → lỗi
- [ ] Reload form → checked đúng môn đã lưu
- [ ] Build pass

**Test Plan:**
1. Chọn IELTS + Toán → save → DB 2 rows  
2. Bỏ Toán, thêm Python → save → set mới đúng  
3. Uncheck all → validation error  

**Ước tính:** 45 phút

---

### M2-T7 — Chứng chỉ gắn TutorProfile (refactor M1-T9)

**Mục tiêu:** Chuyển quản lý chứng chỉ sang ngữ cảnh hồ sơ gia sư; FK `TutorProfileId` (sau T2).

**Scope:**
- Chuyển/adapt actions từ `AccountController` sang `TutorProfileController`:
  - `Certificates` GET, `UploadCertificate` POST, (optional) Delete
- View `Views/TutorProfile/Certificates.cshtml` (có thể move từ Account)
- Query/filter theo `TutorProfileId` của user hiện tại
- Upload JPG/PNG/PDF ≤ 10MB qua `IFileStorageService` → `certificates`
- `IsVerified = false` sau upload; hiển thị badge “Chờ xác minh”
- Redirect `/Account/Certificates` → route mới (301/Redirect) để không gãy bookmark
- Navbar: link Chứng chỉ trỏ route mới

**Out of Scope:**
- Không Admin duyệt (M8)
- Không AI `AiVerifyNote` (T12)
- Không sửa schema thêm field ngoài đã có

**Output cần có:**
- Actions trên `TutorProfileController`
- Views TutorProfile Certificates
- Redirect legacy Account routes
- Dọn code trùng trên AccountController (xóa hoặc thin wrapper)

**Acceptance Criteria:**
- [ ] Tutor có profile upload cert → list theo profile, badge chờ xác minh
- [ ] Tutor chưa có profile → redirect Create
- [ ] Student → 403
- [ ] Legacy `/Account/Certificates` vẫn vào được (redirect)
- [ ] Build pass

**Test Plan:**
1. Tutor + profile → upload PDF → list OK  
2. Sai format / > 10MB → lỗi  
3. Student access → 403  
4. `/Account/Certificates` → redirect  

**Ước tính:** 60 phút

---

### M2-T8 — Quản lý lịch rảnh (weekly schedule)

**Mục tiêu:** Tutor CRUD khung giờ rảnh theo tuần.

**Scope:**
- UI bảng / list 7 ngày + form thêm khung (DayOfWeek, StartTime, EndTime)
- Actions: `Availability` GET, `AddAvailability` POST, `DeleteAvailability` POST
- Validate: Start < End; không cho trùng overlap cùng ngày (khuyến nghị)
- Lưu `TutorAvailabilities`

**Out of Scope:**
- Không timezone phức tạp (lưu local/server time, document)
- Không calendar booking thật (M7)
- Không sync Google Calendar

**Output cần có:**
- `ViewModels/TutorProfile/AvailabilityViewModel.cs` (+ item model)
- Views `Views/TutorProfile/Availability.cshtml`
- Controller actions

**Acceptance Criteria:**
- [ ] Thêm khung hợp lệ → hiển thị đúng ngày
- [ ] Start ≥ End → lỗi
- [ ] Xóa khung → mất khỏi DB
- [ ] Chỉ owner
- [ ] Build pass

**Test Plan:**
1. Thêm T2 18:00–20:00 → list  
2. Thêm 20:00–18:00 → lỗi  
3. Xóa entry → DB empty cho id đó  
4. Student URL → 403  

**Ước tính:** 90 phút

---

### M2-T9 — Demo Lesson CRUD

**Mục tiêu:** Tutor thêm / sửa / xóa bài học mẫu trên hồ sơ.

**Scope:**
- Entity `DemoLesson` đã có từ T2
- Actions: List (trên MyProfile hoặc trang riêng), Create, Edit, Delete
- Fields: Title (required), Description, VideoUrl (URL hoặc upload đơn giản — URL-only OK trong 45m)
- `[Authorize(Roles = "Tutor")]` + owner check

**Out of Scope:**
- Không playlist / comment
- Không AI generate script

**Output cần có:**
- ViewModels DemoLesson
- Views Create/Edit/List (hoặc section trên MyProfile)
- Controller actions

**Acceptance Criteria:**
- [ ] CRUD thành công, gắn đúng `TutorProfileId`
- [ ] Title rỗng → validation
- [ ] Delete không ảnh hưởng demo của tutor khác
- [ ] Build pass

**Test Plan:**
1. Thêm 2 demo → list 2  
2. Sửa title → OK  
3. Xóa 1 → còn 1  

**Ước tính:** 45 phút

---

### M2-T10 — Trang hồ sơ công khai

**Mục tiêu:** Guest/Student/Tutor xem hồ sơ public của gia sư đã được duyệt.

**Scope:**
- Action public: `TutorProfileController.Details(int id)` **hoặc** `Public(int id)` — **không** require auth
- Chỉ show khi `IsVerified == true` (hồ sơ chưa duyệt → 404 cho Guest/Student; owner vẫn xem MyProfile)
- Hiển thị:
  - Avatar (User), Headline, Bio, subjects, experience, education, rate, mode, address (nếu có)
  - Video intro
  - Certificates **đã** `IsVerified`
  - Availabilities
  - Demo lessons
  - Rating placeholder (`AverageRating` / `TotalReviews` — M10 sau)
- Nút “Liên hệ / Thuê gia sư”: chỉ hiện khi `User` login role **Student** (link stub `#` hoặc `/JobPosting` nếu chưa có M3)
- Layout responsive (Bootstrap 5, reuse style M1)

**Out of Scope:**
- Không Review list thật (M10)
- Không chat thật (M6)
- Không Admin ẩn hồ sơ UI (M8) — chỉ check `IsVerified`

**Output cần có:**
- `Views/TutorProfile/Details.cshtml` (public)
- ViewModel read-only aggregate
- Controller action public

**Acceptance Criteria:**
- [ ] Profile `IsVerified=true` → Guest xem được full public info
- [ ] Profile `IsVerified=false` → Guest 404
- [ ] Guest không thấy nút liên hệ; Student đã login thấy nút
- [ ] Chỉ cert đã verify hiện trên public
- [ ] Build pass

**Test Plan:**
1. Seed/manual set `IsVerified=true` → mở Details  
2. Set false → Guest 404  
3. Login Student → có nút liên hệ  
4. Guest → không có nút  

**Ước tính:** 60 phút

---

### M2-T11 — Tìm kiếm & lọc gia sư

**Mục tiêu:** Trang search public với filter + sort + phân trang.

**Scope:**
- Action: `TutorProfileController.Search` (GET, query string)
- Filters (UC-M2-10):
  - SubjectId
  - HourlyRate min–max
  - TeachingMode
  - EducationLevel
  - (Rating min — dùng `AverageRating`, default 0)
  - Keyword (Headline/Bio/FullName) optional
- Chỉ `IsVerified == true` (+ optional `IsAvailable == true` mặc định bật)
- Sort: RatingDesc / PriceAsc / Newest
- Page size 12, phân trang
- URL query string shareable (`?subjectId=1&minRate=100000&page=2`)
- Card kết quả → link Details

**Out of Scope:**
- Không full-text search SQL Server phức tạp
- Không map địa điểm GPS
- Không AI ranking (M4)

**Output cần có:**
- `ViewModels/TutorProfile/TutorSearchViewModel.cs`
- `Views/TutorProfile/Search.cshtml`
- Link navbar “Tìm gia sư” (Guest + authenticated)

**Acceptance Criteria:**
- [ ] Filter môn + khoảng giá trả về đúng subset
- [ ] Phân trang 12/item
- [ ] Copy URL → mở lại cùng filter
- [ ] Không lộ profile `IsVerified=false`
- [ ] Build pass

**Test Plan:**
1. Tạo ≥ 2 profile verified khác môn/giá  
2. Filter môn A → chỉ A  
3. Page size / page 2  
4. Profile chưa duyệt không xuất hiện  

**Ước tính:** 90 phút

---

### M2-T12 — AI kiểm duyệt ảnh/chứng chỉ (stub hoặc Python API)

**Mục tiêu:** Hook kiểm duyệt sau upload chứng chỉ / avatar (theo FRS); có thể **stub** nếu Python API chưa sẵn.

**Scope (chọn 1 path trong prompt, ghi rõ):**
- **Path A — Stub (khuyến nghị nếu chưa có API):**  
  - Interface `IContentModerationService`  
  - Implementation ghi `AiVerifyNote = "Pending AI review (stub)"`, log  
  - Gọi sau upload certificate (và optional avatar)
- **Path B — Real:** HTTP client gọi Python FastAPI endpoint (config `AiService:BaseUrl`), parse response → `AiVerifyNote` + optional auto-reject

**Out of Scope:**
- Không Admin UI duyệt (M8)
- Không train model
- Không block upload nếu service down (fail-open + note) trừ khi FRS siết

**Output cần có:**
- `IContentModerationService` + implementation
- DI + appsettings section (nếu Path B)
- Gọi từ upload certificate flow
- Report ghi rõ stub vs real

**Acceptance Criteria:**
- [ ] Sau upload cert → `AiVerifyNote` được set (stub text hoặc response AI)
- [ ] Lỗi AI service không crash app (log + message mềm)
- [ ] Build pass

**Test Plan:**
1. Upload cert → DB có `AiVerifyNote`  
2. (Path B) Tắt API → app vẫn 200, note/log lỗi  

**Ước tính:** 60 phút

---

## Tổng kết

| Task | Ước tính | Phụ thuộc |
|---|---|---|
| M2-T1 Models core + Seed Subjects | 60 phút | — |
| M2-T2 Availability/Demo + Certificate FK | 60 phút | T1 |
| M2-T3 FileStorageService | 45 phút | T2 |
| M2-T4 Create/Edit Tutor Profile | 90 phút | T3 |
| M2-T5 Video intro | 60 phút | T4 |
| M2-T6 Môn dạy multi-select | 45 phút | T4 |
| M2-T7 Certificates → TutorProfile | 60 phút | T4 |
| M2-T8 Lịch rảnh | 90 phút | T4 |
| M2-T9 Demo Lesson CRUD | 45 phút | T4 |
| M2-T10 Public profile | 60 phút | T6 (+T7–T9 nên có) |
| M2-T11 Search & filter | 90 phút | T10 |
| M2-T12 AI moderation (stub/real) | 60 phút | T7 |
| **Tổng** | **~765 phút (~12.5 giờ)** | |

---

## Out of Scope Module 2 (không break task)

| Hạng mục | Module phụ trách |
|---|---|
| Admin duyệt hồ sơ / chứng chỉ UI | M8 |
| Review & rating thật | M10 |
| Job posting / Apply / Chat / Booking | M3 / M5 / M6 / M7 |
| Livestream, e-contract, portfolio ngoài | Out of FRS |
| OAuth | Out of M1/M2 |

---

## Debt / lưu ý từ M1 cần xử lý trong M2

| Debt | Xử lý ở task |
|---|---|
| `TutorCertificate.UserId` tạm | **M2-T2** (+ UI **M2-T7**) |
| Upload không xóa file cũ | **M2-T3** `DeleteIfExistsAsync` |
| Avatar resize FRS (400×400) | **M2-T3** ImageSharp (optional dùng lại cho Account avatar) |
| Profile Account thiếu DOB/Gender/Address form | Không bắt buộc M2; Address dạy học nằm trên **TutorProfile** |

---

## Quy ước artifact

| Loại | Path |
|---|---|
| Break tasks | `Docs/BreakTasks/BreakTask_M2_TutorProfile.md` |
| FRS | `Docs/FRS/FRS_M2_TutorProfile.md` |
| Prompts | `Docs/Prompts/M2_T1_....xml` … `M2_T12_....xml` |
| Reports | `Docs/Reports/Report_M2_T1_YYYY-MM-DD.md` … |

> **Prompt:** mỗi task 1 file XML, đủ `hardRules` / `scope` / `outputRequired` / `acceptanceCriteria` / `testPlan`.  
> **Không** giao agent cả module; làm tuần tự theo dependency.
