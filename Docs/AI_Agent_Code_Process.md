# Quy Trình Làm Việc với AI Agent Code (Chuẩn MeU)

> Tài liệu này mô tả đầy đủ quy trình làm việc giữa Engineer và AI Agent Code,  
> áp dụng cho dự án **StudyMate** và các dự án thực tế tại MeU.

---

## Tổng Quan Flow

```
┌─────────┐    ┌──────────┐    ┌───────────────┐    ┌────────────┐
│  1. FRS │───▶│ 2. Plan  │───▶│ 3. Break Tasks│───▶│ 4. prompt  │
│         │    │  Setup   │    │  (30-90 phút) │    │    .xml    │
└─────────┘    └──────────┘    └───────────────┘    └─────┬──────┘
                                                           │
┌─────────────┐    ┌──────────┐    ┌──────────┐    ┌──────▼──────┐
│ 8. Evaluate │◀───│ 7. Agent │◀───│ 6. Agent │◀───│ 5. Review   │
│  & Report   │    │   Code   │    │  Reads   │    │   Prompt    │
└─────────────┘    └──────────┘    └──────────┘    └─────────────┘
        │
        ▼
  Feedback & Adjustment (nếu cần) ──▶ Quay lại bước 3
```

---

## Bước 1 — FRS (Functional Requirement Specification)

### Mục đích
Làm rõ **toàn bộ yêu cầu** trước khi bắt tay vào bất kỳ thứ gì.  
Không có FRS rõ ràng → không tạo task → không giao agent.

### Nội dung cần có trong FRS
- **Actors**: Ai dùng hệ thống? (Guest, Student, Tutor, Admin...)
- **Use Cases**: Mỗi actor làm được gì?
- **Business Rules**: Quy tắc nghiệp vụ (VD: lời mời phải chờ xác nhận)
- **Acceptance Criteria**: Điều kiện để tính là "done"
- **Out of Scope**: Những gì KHÔNG làm trong phiên này

### Ví dụ (StudyMate - Module 1)
```
Actors: Guest, Student, Tutor, Admin

Use Cases:
- Guest có thể đăng ký tài khoản bằng email + password
- Hệ thống gửi OTP xác thực email sau khi đăng ký
- User đăng nhập bằng email + password
- User có thể yêu cầu reset mật khẩu qua email

Business Rules:
- OTP có hiệu lực 5 phút
- Tài khoản chưa xác thực email không thể đăng nhập
- Mật khẩu tối thiểu 8 ký tự, có chữ hoa và số

Out of Scope:
- Đăng nhập bằng Google/Facebook
- 2FA
- Bảo mật doanh nghiệp
```

---

## Bước 2 — Plan Setup

### Mục đích
Xác định **kiến trúc tổng thể**, phân layer, xác định module và dependency trước khi chia task.

### Nội dung cần chốt
- Cấu trúc thư mục project
- Tech stack và version
- Database schema (tham chiếu)
- Dependency giữa các module

### Ví dụ (StudyMate)
```
StudyMate/
├── Controllers/        ← Xử lý HTTP request
├── Models/             ← EF Core entities
├── ViewModels/         ← DTO cho Razor View
├── Services/           ← Business logic
│   ├── Interfaces/
│   └── Implementations/
├── Data/               ← DbContext, Migrations
├── Views/              ← Razor Views
│   ├── Shared/
│   ├── Account/
│   └── Home/
└── wwwroot/            ← Bootstrap 5, jQuery, CSS

Tech Stack:
- ASP.NET Core MVC (.NET 10)
- Entity Framework Core + SQL Server (dev)
- ASP.NET Identity
- Bootstrap 5 + jQuery + Ajax
- SignalR (module chat)
```

---

## Bước 3 — Break Down Tasks

### Nguyên tắc vàng
> **Mỗi task = 1 việc cụ thể + 30–90 phút + độc lập**

### Quy tắc độc lập (Task Independence Rule)

> **Task phải độc lập về dependency** — không phụ thuộc vào task khác đang chạy song song.

Nếu Task B cần kết quả của Task A:
- Phải chờ Task A hoàn thành và được review/merge
- Chỉ sau đó mới tạo prompt cho Task B
- Không được chạy song song 2 task có dependency vào nhau

```
❌ Sai: Tạo prompt cho cả Task A và Task B cùng lúc khi B phụ thuộc A
         Task A (đang chạy) ──▶ Task B (chưa có output của A)
                                  └── Agent B tự suy diễn → sai kiến trúc

✅ Đúng: Task A hoàn thành, review pass → mới tạo prompt Task B
         Task A ──▶ Review ──▶ Merge ──▶ Task B (có đủ context)
```

Cách xác định dependency trước khi chia task:
- Task B có đọc file output của Task A không?
- Task B có cần API/service Task A tạo ra không?
- Task B có cần migration/schema Task A tạo không?
→ Có bất kỳ câu nào là Yes → Task B phụ thuộc Task A, không được song song.

### Task phải có đủ 7 thành phần
| Thành phần | Mô tả |
|---|---|
| **Tên task** | Rõ ràng, đọc là hiểu ngay |
| **Mục tiêu** | Làm xong task này đạt được gì? |
| **Scope** | Làm những gì |
| **Out of Scope** | Không làm gì |
| **Output** | File/component cần tạo ra |
| **Acceptance Criteria** | Điều kiện để tính là "done" |
| **Test Plan** | Các bước test cụ thể |

### Tại sao task phải nhỏ?
- Agent khó kiểm soát scope nếu task quá lớn
- Task nhỏ → dễ review, dễ test, dễ rollback khi sai
- Task nhỏ → ước lượng thời gian chính xác hơn
- Task nhỏ → parallel làm được, nhiều người cùng chạy

### ❌ Task sai cách
```
Task: "Làm module Authentication"
→ Quá rộng, agent sẽ tự suy diễn, khó review
```

### ✅ Task đúng cách
```
Task 1: Setup ASP.NET Identity + DbContext (45 phút)
Task 2: Trang Register + Validation (60 phút)
Task 3: Gửi OTP Email sau đăng ký (60 phút)
Task 4: Trang Login + Remember Me (30 phút)
Task 5: Forgot Password flow (60 phút)
```

### Bảng chia task mẫu (StudyMate - Module 1)

| Task | Mô tả | Ước tính |
|---|---|---|
| T1.1 | Setup EF Core + SQL Server connection | 30 phút |
| T1.2 | Cấu hình ASP.NET Identity + ApplicationUser | 45 phút |
| T1.3 | Tạo Migration + Seed Roles | 30 phút |
| T1.4 | Trang Register + Server-side Validation | 60 phút |
| T1.5 | Service gửi OTP qua Email | 60 phút |
| T1.6 | Xác thực OTP sau đăng ký | 45 phút |
| T1.7 | Trang Login + Remember Me | 45 phút |
| T1.8 | Forgot Password + Reset Password flow | 60 phút |

---

## Bước 4 — Tạo prompt.xml

### Mục đích
`prompt.xml` là **file hướng dẫn chuẩn** để giao cho AI Agent.  
Agent đọc file này và biết chính xác phải làm gì, đọc file nào, không được đụng vào đâu.

### Cấu trúc chuẩn

```xml
<prompt>
  <meta>
    <taskName>Tên task ngắn gọn</taskName>
    <timeEstimate>45 minutes</timeEstimate>
    <planReference>Module 1 - Task 1.2</planReference>
  </meta>

  <goal>
    Mô tả ngắn gọn mục tiêu của task này.
    Agent đọc vào phải hiểu ngay cần làm gì.
  </goal>

  <frsReference>
    Trích dẫn phần FRS liên quan đến task này.
    VD: "User đăng ký bằng email + password. Tài khoản chưa xác thực không thể đăng nhập."
  </frsReference>

  <taskUnit>
    <!-- Đơn vị công việc nhỏ nhất của task này — dùng để ước lượng và kiểm soát scope -->
    <!-- Mỗi taskUnit là 1 action độc lập, có thể verify riêng -->
    - Tạo file ApplicationUser.cs
    - Tạo file ApplicationDbContext.cs
    - Cập nhật Program.cs phần cấu hình Identity
  </taskUnit>

  <context>
    <!-- File agent PHẢI đọc trước khi code -->
    <readFile>StudyMate/Data/ApplicationDbContext.cs</readFile>
    <readFile>StudyMate/Models/ApplicationUser.cs</readFile>
    <readFile>StudyMate/Program.cs</readFile>
  </context>

  <scope>
    <!-- Làm đúng những gì này, không thêm không bớt -->
    - Tạo class ApplicationUser kế thừa IdentityUser
    - Thêm các field mở rộng: FullName, AvatarUrl, DateOfBirth, Gender, Address
    - Cấu hình Identity trong Program.cs
    - Tạo ApplicationDbContext kế thừa IdentityDbContext
  </scope>

  <hardRules>
    <!-- Agent KHÔNG được vi phạm -->
    <!-- ⚠️ hardRules là phần quan trọng nhất trong prompt:
         Nếu không có hardRules → agent sẽ tự suy diễn phần còn thiếu,
         tự sửa file ngoài scope, tự thêm feature "tiện thể",
         thay đổi kiến trúc mà engineer không hay biết.
         hardRules = ranh giới không thể vượt qua, dù agent thấy "hợp lý". -->
    - Không sửa appsettings.json
    - Không tạo Controller hay View trong task này
    - Không thêm package ngoài danh sách đã có
    - Không scaffold, viết tay toàn bộ
  </hardRules>

  <tasks>
    <!-- Liệt kê từng bước thực hiện theo thứ tự — agent đi theo đúng thứ tự này -->
    1. Đọc ApplicationDbContext.cs và ApplicationUser.cs hiện tại
    2. Tóm tắt lại kiến trúc hiện có cho engineer xác nhận
    3. Tạo/cập nhật ApplicationUser với các field mở rộng
    4. Tạo/cập nhật ApplicationDbContext kế thừa IdentityDbContext
    5. Cập nhật Program.cs: đăng ký Identity service
    6. Self-review: đảm bảo không sửa file ngoài outputRequired
    7. Báo cáo thay đổi và rủi ro
  </tasks>

  <constraints>
    - Dùng .NET 10 / ASP.NET Core MVC
    - Tuân theo cấu trúc thư mục đã định nghĩa trong Plan
    - Code phải có XML comment cho public members
  </constraints>

  <acceptanceCriteria>
    - ApplicationUser.cs tồn tại với đầy đủ các field mở rộng
    - Program.cs đã cấu hình Identity với đúng options
    - ApplicationDbContext.cs kế thừa IdentityDbContext<ApplicationUser>
    - Project build thành công, không có warning
  </acceptanceCriteria>

  <testPlan>
    - Chạy dotnet build → phải thành công
    - Chạy dotnet ef migrations add InitIdentity → migration tạo ra đúng bảng
    - Kiểm tra ApplicationUser có đủ field trong migration
  </testPlan>

  <outputRequired>
    - StudyMate/Models/ApplicationUser.cs (tạo mới)
    - StudyMate/Data/ApplicationDbContext.cs (tạo mới)
    - StudyMate/Program.cs (chỉnh sửa phần Identity)
  </outputRequired>
</prompt>
```

### Checklist prompt trước khi giao agent

- [ ] Có `<goal>` rõ ràng, đọc một câu là hiểu
- [ ] Có `<context>` chỉ rõ file cần đọc
- [ ] Có `<scope>` liệt kê cụ thể từng việc
- [ ] Có `<hardRules>` nói rõ không được làm gì — **không được bỏ trống**
- [ ] Có `<acceptanceCriteria>` đo được, không mơ hồ
- [ ] Có `<testPlan>` với bước test cụ thể
- [ ] Ước lượng thời gian nằm trong 30–90 phút
- [ ] Task không phụ thuộc task khác đang chạy song song

---

## Bước 5 — Review Prompt

### Mục đích
Engineer **đọc lại prompt trước khi giao** để đảm bảo agent không bị mơ hồ.

### Checklist review prompt

**Về Scope:**
- [ ] Scope có bị overlap với task khác không?
- [ ] Scope có quá lớn, vượt 90 phút không?
- [ ] Output có rõ ràng, đo được không?

**Về Context:**
- [ ] Đã liệt kê đủ file agent cần đọc chưa?
- [ ] File tham chiếu có tồn tại thực tế không?

**Về Hard Rules:**
- [ ] Đã chặn những phần không được đụng vào chưa?
- [ ] Có constraint về tech stack, version chưa?

**Về Test Plan:**
- [ ] Test plan có thực hiện được không?
- [ ] Acceptance Criteria có mơ hồ không? (tránh "hoạt động tốt", "đẹp"...)

---

## Bước 6 — Agent Đọc & Hiểu Source

### Quy trình agent thực hiện
1. Đọc toàn bộ `prompt.xml`
2. Đọc các file trong `<context>`
3. **Tóm tắt lại bằng lời** những gì đã hiểu về source hiện tại — bắt buộc, không được bỏ qua
4. Tạo mini plan cho phần implementation
5. Xác nhận scope với engineer nếu có điểm chưa rõ
6. Bắt đầu code **chỉ sau khi engineer xác nhận tóm tắt là đúng**

### Tại sao bước tóm tắt là bắt buộc?

> Nếu agent không tóm tắt lại, engineer không biết agent đang hiểu đúng hay sai kiến trúc.  
> Agent có thể đọc đủ file nhưng vẫn hiểu sai pattern, sai naming convention, sai flow nghiệp vụ.  
> Tóm tắt = bước kiểm tra sớm nhất — phát hiện hiểu sai **trước khi code** thay vì phát hiện sau khi đã viết 200 dòng.

### Tóm tắt cần có gì?
Agent phải tóm tắt đủ 3 phần:

```
1. Kiến trúc hiện tại:
   "Project dùng ASP.NET Core MVC, DbContext là ApplicationDbContext,
    ApplicationUser đã kế thừa IdentityUser với các field: FullName, AvatarUrl..."

2. Những gì task này cần làm:
   "Task yêu cầu tạo AccountController với action Register,
    nhận RegisterViewModel, validate, tạo user qua UserManager..."

3. Những gì KHÔNG được làm:
   "Không sửa Program.cs, không tạo migration, không đụng View layout..."
```

### Engineer cần làm gì ở bước này?
- **Đọc kỹ tóm tắt của agent** — không chỉ nhìn qua
- Xác nhận agent đã hiểu đúng kiến trúc và scope
- Nếu tóm tắt sai → **điều chỉnh ngay, không cho code tiếp**
- Nếu tóm tắt đúng → cho phép agent tiến hành bước 7
- **Không để agent tự suy diễn** những điểm mơ hồ

---

## Bước 7 — Agent Code

### Nguyên tắc
- Code **đúng scope**, không thêm feature tự phát
- Giải thích từng thay đổi quan trọng
- Self-review trước khi trả kết quả
- Chạy build/test nếu có thể
- Báo rõ những phần còn rủi ro hoặc chưa chắc

### Agent KHÔNG được tự ý
- Thêm package mới không có trong prompt
- Sửa file ngoài `<outputRequired>`
- Thay đổi kiến trúc hoặc naming convention
- Bỏ qua `<hardRules>`

---

## Bước 8 — Engineer Review, Test & Build

### Quy trình review

```
1. Đọc diff từng file thay đổi
2. Đối chiếu với Acceptance Criteria
3. Chạy build
4. Chạy test plan
5. Kiểm tra không có regression
6. Accept hoặc yêu cầu sửa
```

### ❌ Không được làm
```
- Copy/paste kết quả agent mà không đọc
- Accept khi chưa build thành công
- Bỏ qua test plan
```

### ✅ Phải làm
```
- Đọc từng dòng code thay đổi
- Hiểu được tại sao agent viết vậy
- Có thể giải thích lại cho người khác
```

### Checklist sau khi agent code

- [ ] Build thành công (`dotnet build`)
- [ ] Không có error, warning nghiêm trọng
- [ ] Tất cả Acceptance Criteria đều đạt
- [ ] Test Plan đã chạy đủ các bước
- [ ] Không có thay đổi ngoài scope
- [ ] Code tuân thủ naming convention của project
- [ ] Không có hardcode credential hoặc magic number

---

## Bước 9 — Evaluate & Report

### Báo cáo sau mỗi task (bắt buộc)

```markdown
## Report - [Tên Task] - [Ngày]

### Đã làm gì
- Liệt kê file đã tạo/sửa
- Mô tả thay đổi chính

### Kết quả
- Build: ✅ Pass / ❌ Fail
- Test Plan: ✅ Pass / ❌ Fail (bước nào fail?)
- Acceptance Criteria: ✅ Đạt / ❌ Chưa đạt

### Rủi ro phát hiện
- Vấn đề kỹ thuật gặp phải
- Điểm có thể gây ra lỗi sau này

### Hiểu được gì
- Kiến thức/pattern học được từ task này

### Bước tiếp theo
- Task tiếp theo là gì?
- Cần chuẩn bị gì cho task đó?
```

---

## Vòng Lặp Feedback & Adjustment

Nếu output không đạt Acceptance Criteria:

```
Kết quả không đạt AC
         │
         ▼
  Phân tích nguyên nhân
    - Prompt thiếu rõ ràng?
    - Scope quá lớn, vượt 90 phút?
    - Context chưa đủ file cần đọc?
    - hardRules chưa chặn đủ?
    - Agent tóm tắt sai, engineer không phát hiện sớm?
         │
         ▼
  Điều chỉnh prompt.xml
  (bổ sung hardRules, thu hẹp scope, thêm context)
         │
         ▼
  Giao lại agent — không sửa task đang dở giữa chừng
         │
         ▼
  Agent tóm tắt lại → Engineer xác nhận → Agent code
         │
         ▼
  Review lại từ đầu theo checklist
```

### Quy tắc khi task fail 2 lần liên tiếp

> **Dừng ngay. Không giao lần 3.**

Thay vào đó:
1. Gặp lead/EM báo cáo tình trạng
2. Phân tích root cause: prompt sai hay task quá lớn?
3. Nếu task quá lớn → **chia nhỏ hơn nữa**, mỗi task ≤ 30 phút
4. Nếu prompt sai → viết lại từ đầu, không patch prompt cũ
5. Cân nhắc làm thủ công nếu agent không kiểm soát được scope

### Dấu hiệu cần kích hoạt Feedback Loop ngay

| Dấu hiệu | Hành động |
|---|---|
| Agent sửa file ngoài `<outputRequired>` | Dừng, review lại hardRules |
| Agent thêm package không được yêu cầu | Dừng, revert, bổ sung hardRules |
| Build fail sau khi agent code | Không accept, yêu cầu agent fix trong scope |
| Agent tóm tắt sai kiến trúc | Không cho code, điều chỉnh context |
| Output > 90 phút vẫn chưa xong | Dừng, chia task nhỏ hơn |

---

## Rủi Ro Thường Gặp & Cách Xử Lý

| Rủi ro | Dấu hiệu | Cách xử lý |
|---|---|---|
| Task quá lớn | Agent code vượt 90 phút, output lan rộng | Chia nhỏ lại trước khi giao |
| Prompt mơ hồ | Agent tự thêm feature không yêu cầu | Thêm `<hardRules>` cụ thể hơn |
| Thiếu hardRules | Agent sửa file ngoài scope "vì tiện thể" | Liệt kê rõ từng file/action không được làm |
| Thiếu context | Agent hiểu sai kiến trúc hiện có | Bổ sung file cần đọc vào `<context>` |
| Bỏ qua bước tóm tắt | Agent code sai hướng 200 dòng trước khi phát hiện | Bắt buộc agent tóm tắt, engineer xác nhận trước khi code |
| Task có dependency | Task B chạy song song với Task A đang dở | Chờ Task A merge xong mới tạo prompt Task B |
| Thiếu review | Bug lọt qua, khó trace sau này | Bắt buộc đọc diff trước khi merge |
| Thiếu test | Không biết task đúng hay sai | Viết test plan cụ thể, chạy đủ bước |
| Scope creep | Agent sửa file ngoài scope | Liệt kê rõ `<outputRequired>` |

---

## Mindset Làm Việc với AI Agent Code

> AI Agent là công cụ hỗ trợ — không phải người thay thế engineer.  
> Engineer vẫn là người chịu trách nhiệm toàn bộ output.

### 7 Nguyên Tắc Mindset Bắt Buộc

**1. AI Agent không thay thế trách nhiệm của engineer**
- Agent code ra gì, engineer chịu trách nhiệm về cái đó
- Không được báo "Done" khi chỉ mới chạy agent xong
- Output của agent = bản nháp, engineer mới là người quyết định accept hay không

**2. Engineer phải hiểu requirement trước khi prompt**
- Không đọc FRS → không tạo prompt
- Không hiểu business rule → không chia task
- Giao agent khi chưa hiểu requirement = kết quả không kiểm soát được

**3. Engineer phải kiểm soát scope**
- Scope trong prompt phải do engineer định nghĩa, không để agent tự mở rộng
- Nếu agent làm thêm ngoài scope → revert, không accept dù kết quả "có vẻ đúng"
- Scope creep nhỏ hôm nay = technical debt lớn tuần sau

**4. Engineer phải review code trước khi accept**
- Đọc từng dòng diff, không chỉ đọc tóm tắt của agent
- Hiểu được tại sao agent viết vậy, không chỉ biết nó làm gì
- Nếu có dòng code không giải thích được → hỏi, không accept

**5. Engineer phải test/build trước khi báo Done**
- Build pass là điều kiện tối thiểu, không phải điều kiện đủ
- Chạy đủ test plan trong prompt trước khi đóng task
- Checklist review phải được tick đủ, không tick đại

**6. Engineer phải giải thích được thay đổi agent đã làm**
- Nếu team lead hỏi "tại sao file này thay đổi?" phải trả lời được ngay
- Không merge khi còn điểm nào trong output chưa giải thích được
- Đây là dấu hiệu rõ nhất phân biệt engineer thật sự với người chỉ copy-paste

**7. Không merge hoặc handoff khi chưa hiểu thay đổi**
- Merge = cam kết với cả team rằng code này đúng và an toàn
- Handoff = chuyển giao trách nhiệm — phải hiểu mới chuyển được
- Thà chậm 1 giờ để hiểu còn hơn merge sai gây block cả team

---

## Key Principles (Nguyên Tắc Cốt Lõi)

> 1. **Hiểu kiến trúc hệ thống** trước khi viết bất kỳ dòng code nào
> 2. **Giải thích mọi thay đổi** — không code âm thầm
> 3. **Luôn dùng prompt.xml** làm input chuẩn cho agent
> 4. **Tuân thủ code standards** của project
> 5. **Giữ scope trong giới hạn** — không tự thêm feature
> 6. **Task lý tưởng: 30–90 phút** — không phù hợp nếu > 90 phút

---

## Áp Dụng vào StudyMate

### Thứ tự thực hiện theo module

```
Module 1: Auth & Account     ← Bắt đầu từ đây
    ↓
Module 2: Tutor Profile
    ↓
Module 3: Job Posting
    ↓
Module 4: AI Matching
    ↓
Module 5: Application Flow
    ↓
Module 6: Chat (SignalR)
    ↓
Module 7: Booking / Calendar
    ↓
Module 8: Admin Dashboard
    ↓
Module 9: Payment
    ↓
Module 10: Review & Rating
    ↓
Module 11: Notification
    ↓
Module 12: AI Learning Assistant
```

### Thư mục tổ chức tài liệu

```
StudyMate/
└── Docs/
    ├── AI_Agent_Code_Process.md   ← File này
    ├── FRS/
    │   ├── FRS_Module1_Auth.md
    │   ├── FRS_Module2_TutorProfile.md
    │   └── ...
    ├── Prompts/
    │   ├── M1_T1_Setup_Identity.xml
    │   ├── M1_T2_Register_Page.xml
    │   └── ...
    └── Reports/
        ├── Report_M1_T1_[date].md
        └── ...
```

---

## Weekly Report Format

### 1. Completed this week
- Đã đọc và hiểu những phần nào của quy trình
- Đã tạo task nhỏ nào
- Đã tạo prompt.xml nào
- Đã thực hành với agent trên task nào

### 2. Key learning
- Em hiểu gì về cách làm việc với AI Agent Code
- Em hiểu gì về việc chia task 30–90 phút
- Em hiểu gì về trách nhiệm review/test của engineer

### 3. Issues / blockers
- Phần nào chưa rõ
- Prompt phần nào còn khó viết
- Khi agent code có vấn đề gì
- **Output của agent có điểm nào mình chưa giải thích được không?** ← nếu có → chưa được phép merge
- Cần EM/Lead hỗ trợ gì

### 4. Plan for next week
- Áp dụng quy trình vào task kỹ thuật thật
- Cải thiện prompt.xml
- Chuẩn hóa checklist review/test
- Báo cáo kết quả theo từng task

---

*Tài liệu này được duy trì và cập nhật theo quá trình phát triển dự án StudyMate.*  
*Phiên bản: 1.2 — Bổ sung taskUnit, tasks tag, Mindset section đầy đủ 7 điểm*
