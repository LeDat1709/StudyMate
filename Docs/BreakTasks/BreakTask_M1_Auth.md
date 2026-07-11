# Break Task — Module 1: Quản Lý Tài Khoản & Xác Thực

> **Module:** M1  
> **FRS Reference:** `Docs/FRS/FRS_M1_Auth.md`  
> **Plan Reference:** `Docs/PlanSetup.md`  
> **Trạng thái đã có sẵn:**
> - ✅ `ApplicationUser.cs` — đã có đủ field
> - ✅ `ApplicationDbContext.cs` — đã cấu hình Identity + OtpCodes
> - ✅ `OtpCode.cs` — đã có đủ field
> - ✅ Migration `InitIdentity` — đã apply
> - ✅ Seed Roles: Admin / Tutor / Student / Guest
> - ✅ `appsettings.json` — đã có ConnectionString, Email, OTP config

---

## Dependency giữa các Task

```
M1-T1 (EmailService)
  │
  └──▶ M1-T2 (OtpService)          ← cần EmailService
          │
          ├──▶ M1-T3 (Register)    ← cần OtpService để gửi OTP
          │         │
          │         └──▶ M1-T4 (VerifyEmail)   ← cần OtpService + user chưa verified
          │
          ├──▶ M1-T5 (Login)       ← độc lập, chỉ cần Identity
          │
          ├──▶ M1-T6 (ForgotPassword)  ← cần OtpService
          │
          └──▶ M1-T7 (Profile + Avatar)    ← cần user đã login
                    │
                    └──▶ M1-T8 (ChangePassword)  ← cần user đã login
                    └──▶ M1-T9 (Tutor Certificate Upload)  ← cần user là Tutor
```

**Thứ tự bắt buộc:**
`T1 → T2 → T3 → T4` (chuỗi liên tiếp)  
`T2 → T5` (T5 có thể làm song song với T3 vì T5 không cần OtpService)  
`T2 → T6` (sau T4 vì dùng chung OtpService pattern)  
`T4 → T7 → T8, T9` (cần user đã verified mới test được)

---

## Danh sách Task

| Task ID | Tên Task | Phụ thuộc | Ước tính | UC liên quan |
|---|---|---|---|---|
| M1-T1 | Email Service (SMTP) | — | 45 phút | UC-M1-01, 02, 05 |
| M1-T2 | OTP Service | M1-T1 | 45 phút | UC-M1-01, 02, 05 |
| M1-T3 | Trang Register + ViewModels + Validation | M1-T2 | 60 phút | UC-M1-01 |
| M1-T4 | Trang VerifyEmail + Logic xác thực OTP | M1-T3 | 60 phút | UC-M1-02 |
| M1-T5 | Trang Login + Remember Me + Logout | M1-T2 | 45 phút | UC-M1-03, 04 |
| M1-T6 | Forgot Password + VerifyOTP + Reset Password | M1-T4, T5 | 90 phút | UC-M1-05 |
| M1-T7 | Trang Profile + Upload Avatar | M1-T5 | 60 phút | UC-M1-06, 07 |
| M1-T8 | Trang Change Password | M1-T7 | 30 phút | UC-M1-08 |
| M1-T9 | Upload Chứng chỉ (Tutor only) | M1-T7 | 45 phút | UC-M1-09 |

---

## Chi tiết từng Task

---

### M1-T1 — Email Service (SMTP)

**Mục tiêu:** Tạo service gửi email qua SMTP, dùng chung cho OTP verify và reset password.

**Scope:**
- Tạo interface `IEmailService` trong `Services/Interfaces/`
- Tạo class `EmailService` trong `Services/Implementations/`
- Đọc config từ `appsettings.json` (SmtpHost, SmtpPort, SenderEmail, Password)
- Đăng ký DI trong `Program.cs`
- Cài package `MailKit` nếu chưa có

**Out of Scope:**
- Không tạo template HTML phức tạp (plain text hoặc HTML đơn giản là đủ)
- Không làm queue email, retry logic
- Không tạo Controller hay View

**Output cần có:**
- `Services/Interfaces/IEmailService.cs`
- `Services/Implementations/EmailService.cs`
- `Program.cs` — thêm `builder.Services.AddScoped<IEmailService, EmailService>()`

**Acceptance Criteria:**
- [ ] `IEmailService` có method `SendEmailAsync(string to, string subject, string body)`
- [ ] `EmailService` đọc config từ `IConfiguration`, không hardcode
- [ ] DI đăng ký đúng, inject được vào constructor
- [ ] Project build thành công sau khi thêm

**Test Plan:**
1. Chạy `dotnet build` → pass
2. Inject `IEmailService` vào `HomeController` test, gọi thử `SendEmailAsync` → không có runtime exception
3. Kiểm tra email nhận được (dùng dev email test)

**Ước tính:** 45 phút

---

### M1-T2 — OTP Service

**Mục tiêu:** Tạo service sinh OTP, lưu vào DB, xác thực OTP — dùng chung cho EmailVerify và ForgotPassword.

**Scope:**
- Tạo interface `IOtpService` trong `Services/Interfaces/`
- Tạo class `OtpService` trong `Services/Implementations/`
- Methods cần có:
  - `GenerateAndSaveOtpAsync(string userId, string purpose)` → trả về mã OTP
  - `ValidateOtpAsync(string userId, string code, string purpose)` → `OtpValidationResult`
  - `InvalidateAllOtpAsync(string userId, string purpose)` → vô hiệu hóa OTP cũ
- Đăng ký DI trong `Program.cs`

**Out of Scope:**
- Không gửi email trong service này (đó là việc của `IEmailService`)
- Không tạo Controller hay View
- Không sửa `OtpCode.cs` hay `ApplicationDbContext.cs`

**Output cần có:**
- `Services/Interfaces/IOtpService.cs`
- `Services/Implementations/OtpService.cs`
- `Program.cs` — thêm `builder.Services.AddScoped<IOtpService, OtpService>()`

**Business Rules cần implement:**
- OTP gồm 6 chữ số ngẫu nhiên
- `Purpose = "EmailVerify"` → hết hạn sau 5 phút
- `Purpose = "ForgotPassword"` → hết hạn sau 10 phút
- Tối đa 3 lần nhập sai → trả về `TooManyAttempts`
- Khi tạo OTP mới → tự động `InvalidateAllOtpAsync` OTP cũ cùng purpose

**Acceptance Criteria:**
- [ ] `GenerateAndSaveOtpAsync` tạo OTP đúng 6 số, lưu vào bảng `OtpCodes`
- [ ] `ValidateOtpAsync` trả về đúng trạng thái: `Valid`, `Invalid`, `Expired`, `AlreadyUsed`, `TooManyAttempts`
- [ ] OTP cũ bị vô hiệu khi tạo OTP mới cùng purpose
- [ ] Project build thành công

**Test Plan:**
1. Chạy `dotnet build` → pass
2. Unit test thủ công: gọi `GenerateAndSaveOtpAsync` → kiểm tra DB có record mới
3. Gọi `ValidateOtpAsync` với mã đúng → `Valid`
4. Gọi `ValidateOtpAsync` với mã sai 3 lần → `TooManyAttempts`

**Ước tính:** 45 phút

---

### M1-T3 — Trang Register + ViewModels + Validation

**Mục tiêu:** Tạo trang đăng ký tài khoản với form validation, tạo user qua Identity, gửi OTP email.

**Scope:**
- Tạo `ViewModels/Account/RegisterViewModel.cs`
- Tạo `Controllers/AccountController.cs` với action `Register` (GET + POST)
- Tạo `Views/Account/Register.cshtml`
- Inject `UserManager`, `IEmailService`, `IOtpService`
- Sau submit thành công: tạo user với `EmailConfirmed = false`, gán role, gửi OTP, redirect `VerifyEmail`

**Out of Scope:**
- Không làm trang VerifyEmail (task tiếp theo)
- Không làm Login
- Không sửa `ApplicationUser.cs` hay `ApplicationDbContext.cs`
- Không làm UI layout cho toàn site

**Output cần có:**
- `ViewModels/Account/RegisterViewModel.cs`
- `Controllers/AccountController.cs` (action Register GET + POST)
- `Views/Account/Register.cshtml`

**Business Rules cần implement:**
- Email phải hợp lệ và chưa tồn tại trong DB
- Mật khẩu: tối thiểu 8 ký tự, có ít nhất 1 chữ hoa, 1 số
- Xác nhận mật khẩu phải khớp
- Vai trò mặc định: Student nếu không chọn
- Tạo user với `IsEmailVerified = false`, `EmailConfirmed = false`

**Acceptance Criteria:**
- [ ] Form hiển thị đúng các field: FullName, Email, Password, ConfirmPassword, Role
- [ ] Validation lỗi hiện inline, không reload trang (jQuery Unobtrusive)
- [ ] Submit thành công → user được tạo trong DB với `EmailConfirmed = false`
- [ ] OTP email được gửi đi
- [ ] Redirect đến `/Account/VerifyEmail`
- [ ] Build pass

**Test Plan:**
1. Chạy `dotnet build` → pass
2. Truy cập `/Account/Register` → form hiển thị đúng
3. Submit form thiếu field → lỗi inline hiện ra, không redirect
4. Submit với email đã tồn tại → lỗi "Email đã được sử dụng"
5. Submit hợp lệ → kiểm tra DB có user mới, `EmailConfirmed = false`
6. Kiểm tra email OTP được gửi đến hộp thư

**Ước tính:** 60 phút

---

### M1-T4 — Trang VerifyEmail + Logic xác thực OTP

**Mục tiêu:** Tạo trang nhập OTP xác thực email, xử lý logic validate, gửi lại OTP.

**Scope:**
- Tạo `ViewModels/Account/VerifyOtpViewModel.cs`
- Thêm actions `VerifyEmail` (GET + POST) vào `AccountController`
- Thêm action `ResendOtp` (POST) vào `AccountController`
- Tạo `Views/Account/VerifyEmail.cshtml`
- Sau OTP đúng: set `EmailConfirmed = true`, `IsEmailVerified = true`, đánh dấu OTP `IsUsed = true`, tự động sign in

**Out of Scope:**
- Không sửa `OtpService` hay `EmailService`
- Không làm trang Login hay Register
- Không sửa layout chung

**Output cần có:**
- `ViewModels/Account/VerifyOtpViewModel.cs`
- `Controllers/AccountController.cs` — thêm `VerifyEmail` GET/POST, `ResendOtp` POST
- `Views/Account/VerifyEmail.cshtml`

**Business Rules cần implement:**
- Hiển thị lỗi đúng theo `OtpValidationResult`: sai / hết hạn / đã dùng / quá số lần
- Nút "Gửi lại OTP" → gọi `InvalidateAllOtpAsync` + tạo OTP mới + gửi email
- Sau xác thực thành công → `SignInManager.SignInAsync` tự động

**Acceptance Criteria:**
- [ ] Nhập OTP đúng → tài khoản `EmailConfirmed = true`, tự động login, redirect trang chủ
- [ ] Nhập sai → hiển thị lỗi, form không bị clear
- [ ] OTP hết hạn → hiển thị lỗi + nút "Gửi lại OTP"
- [ ] Nút Gửi lại OTP hoạt động, OTP mới gửi trong vòng 30 giây
- [ ] Build pass

**Test Plan:**
1. `dotnet build` → pass
2. Đăng ký tài khoản mới → redirect đến VerifyEmail
3. Nhập OTP sai → lỗi hiển thị, không redirect
4. Nhập OTP hết hạn (chờ > 5 phút hoặc mock expired) → đúng trạng thái
5. Nhập OTP đúng → `EmailConfirmed = true` trong DB, redirect trang chủ
6. Click "Gửi lại OTP" → OTP mới trong email, OTP cũ không còn hợp lệ

**Ước tính:** 60 phút

---

### M1-T5 — Trang Login + Remember Me + Logout

**Mục tiêu:** Tạo trang đăng nhập với checkbox Remember Me và action Logout.

**Scope:**
- Tạo `ViewModels/Account/LoginViewModel.cs`
- Thêm actions `Login` (GET + POST), `Logout` (POST) vào `AccountController`
- Tạo `Views/Account/Login.cshtml`
- Xử lý redirect về returnUrl sau login thành công

**Out of Scope:**
- Không làm Forgot Password
- Không sửa `ApplicationUser.cs`
- Không làm OAuth (Google/Facebook)

**Output cần có:**
- `ViewModels/Account/LoginViewModel.cs`
- `Controllers/AccountController.cs` — thêm `Login` GET/POST, `Logout` POST
- `Views/Account/Login.cshtml`

**Business Rules cần implement:**
- Tài khoản chưa `EmailConfirmed` → lỗi "Vui lòng xác thực email" + link resend OTP
- Tài khoản `IsActive = false` → lỗi "Tài khoản bị khóa"
- Sai thông tin → lỗi chung, không nói rõ field nào sai
- Remember Me checked → persistent cookie 30 ngày
- Remember Me unchecked → session cookie

**Acceptance Criteria:**
- [ ] Đăng nhập đúng → redirect đúng returnUrl hoặc trang chủ
- [ ] Đăng nhập sai → lỗi chung, không lộ thông tin
- [ ] Tài khoản chưa verify → lỗi + hướng dẫn verify
- [ ] Remember Me hoạt động đúng (kiểm tra cookie expires)
- [ ] Logout → cookie xóa, redirect trang chủ, không truy cập được trang auth
- [ ] Build pass

**Test Plan:**
1. `dotnet build` → pass
2. Truy cập `/Account/Login` → form hiển thị
3. Nhập sai email/password → lỗi chung
4. Đăng nhập với tài khoản chưa verify → lỗi + link
5. Đăng nhập đúng, không tick Remember Me → đóng browser → phải login lại
6. Đăng nhập đúng, tick Remember Me → đóng browser → vẫn còn session
7. Logout → kiểm tra cookie, truy cập `/Account/Profile` → redirect login

**Ước tính:** 45 phút

---

### M1-T6 — Forgot Password + Verify OTP + Reset Password

**Mục tiêu:** Tạo toàn bộ flow quên mật khẩu gồm 3 bước: nhập email → xác thực OTP → đặt mật khẩu mới.

**Scope:**
- Tạo `ViewModels/Account/ForgotPasswordViewModel.cs`
- Tạo `ViewModels/Account/ResetPasswordViewModel.cs`
- Thêm actions vào `AccountController`:
  - `ForgotPassword` GET + POST
  - `VerifyResetOtp` GET + POST
  - `ResetPassword` GET + POST
- Tạo views tương ứng trong `Views/Account/`

**Out of Scope:**
- Không sửa `OtpService` hay `EmailService`
- Không làm thay đổi profile hay avatar

**Output cần có:**
- `ViewModels/Account/ForgotPasswordViewModel.cs`
- `ViewModels/Account/ResetPasswordViewModel.cs`
- `Controllers/AccountController.cs` — thêm 6 actions
- `Views/Account/ForgotPassword.cshtml`
- `Views/Account/VerifyResetOtp.cshtml`
- `Views/Account/ResetPassword.cshtml`

**Business Rules cần implement:**
- Email không tồn tại → vẫn hiển thị "Nếu email tồn tại, chúng tôi sẽ gửi OTP" (không lộ thông tin)
- OTP reset password có hiệu lực 10 phút (Purpose = `"ForgotPassword"`)
- Sau reset thành công → `SignInManager.SignOutAsync()` all sessions + tự động sign in lại

**Acceptance Criteria:**
- [ ] Flow 3 bước hoạt động đầy đủ từ đầu đến cuối
- [ ] Email không tồn tại → không lộ thông tin
- [ ] OTP hết hạn sau đúng 10 phút
- [ ] Mật khẩu cũ không còn dùng được sau khi reset
- [ ] Build pass

**Test Plan:**
1. `dotnet build` → pass
2. Truy cập `/Account/ForgotPassword`, nhập email không tồn tại → thông báo chung
3. Nhập email đúng → OTP gửi đến email
4. Nhập OTP sai → lỗi
5. Nhập OTP đúng → redirect ResetPassword
6. Nhập mật khẩu mới, login lại với mật khẩu mới → thành công
7. Login với mật khẩu cũ → thất bại

**Ước tính:** 90 phút

---

### M1-T7 — Trang Profile + Upload Avatar

**Mục tiêu:** Tạo trang xem/sửa thông tin cá nhân và upload ảnh đại diện.

**Scope:**
- Tạo `ViewModels/Account/ProfileViewModel.cs`
- Thêm actions `Profile` (GET + POST), `UploadAvatar` (POST) vào `AccountController`
- Tạo `Views/Account/Profile.cshtml`
- Lưu file avatar vào `wwwroot/uploads/avatars/`
- Cập nhật `AvatarUrl` vào `ApplicationUser`

**Out of Scope:**
- Không làm upload chứng chỉ (đó là T9)
- Không sửa Email (hiển thị read-only)
- Không làm đổi mật khẩu từ trang này (đó là T8)

**Output cần có:**
- `ViewModels/Account/ProfileViewModel.cs`
- `Controllers/AccountController.cs` — thêm `Profile` GET/POST, `UploadAvatar` POST
- `Views/Account/Profile.cshtml`
- Thư mục `wwwroot/uploads/avatars/` (tạo nếu chưa có)

**Business Rules cần implement:**
- Chỉ cho phép định dạng: JPG, PNG, WEBP
- Kích thước tối đa: 2MB
- Tên file lưu: `{userId}_{timestamp}.{ext}` (tránh trùng)
- Email field: hiển thị nhưng `disabled`, không submit về server

**Acceptance Criteria:**
- [ ] Hiển thị đúng thông tin hiện tại của user
- [ ] Lưu thành công → thông báo "Cập nhật thành công"
- [ ] Upload avatar → ảnh mới hiển thị ngay không cần reload trang
- [ ] File > 2MB → lỗi rõ ràng
- [ ] File sai định dạng → lỗi rõ ràng
- [ ] Build pass

**Test Plan:**
1. `dotnet build` → pass
2. Truy cập `/Account/Profile` khi đã login → thông tin hiển thị đúng
3. Sửa FullName, Phone → lưu → reload → dữ liệu mới
4. Upload avatar PNG < 2MB → hiển thị ảnh mới
5. Upload file .exe → lỗi định dạng
6. Upload file > 2MB → lỗi kích thước
7. Truy cập `/Account/Profile` khi chưa login → redirect login

**Ước tính:** 60 phút

---

### M1-T8 — Trang Change Password

**Mục tiêu:** Tạo trang đổi mật khẩu cho user đã đăng nhập.

**Scope:**
- Thêm action `ChangePassword` (GET + POST) vào `AccountController`
- Tạo `Views/Account/ChangePassword.cshtml`
- Tạo `ViewModels/Account/ChangePasswordViewModel.cs`

**Out of Scope:**
- Không làm Forgot Password (đã có T6)
- Không làm thay đổi profile

**Output cần có:**
- `ViewModels/Account/ChangePasswordViewModel.cs`
- `Controllers/AccountController.cs` — thêm `ChangePassword` GET/POST
- `Views/Account/ChangePassword.cshtml`

**Business Rules cần implement:**
- Phải xác minh đúng mật khẩu hiện tại trước khi đổi
- Mật khẩu mới không được trùng mật khẩu hiện tại
- Mật khẩu mới phải đáp ứng độ phức tạp (8 ký tự, chữ hoa, số)

**Acceptance Criteria:**
- [ ] Sai mật khẩu hiện tại → lỗi rõ ràng
- [ ] Mật khẩu mới trùng cũ → lỗi
- [ ] Đổi thành công → thông báo thành công
- [ ] Login bằng mật khẩu cũ → thất bại
- [ ] Build pass

**Test Plan:**
1. `dotnet build` → pass
2. Truy cập `/Account/ChangePassword` khi đã login
3. Nhập sai mật khẩu hiện tại → lỗi
4. Nhập mật khẩu mới giống cũ → lỗi
5. Nhập đúng toàn bộ → thành công
6. Logout, login lại bằng mật khẩu mới → thành công
7. Login lại bằng mật khẩu cũ → thất bại

**Ước tính:** 30 phút

---

### M1-T9 — Upload Chứng Chỉ (Tutor only)

**Mục tiêu:** Tutor upload bằng cấp/chứng chỉ, hiển thị danh sách với trạng thái chờ xác minh.

> ⚠️ Task này cần model `TutorCertificate` — tuy nhiên model đó thuộc Module 2.  
> **Nếu M2 chưa có `TutorCertificate.cs`** → task này cần tạo model tạm thời hoặc chờ M2 hoàn thành.  
> Xác nhận với Lead trước khi bắt đầu task này.

**Scope:**
- Tạo model `TutorCertificate.cs` trong `Models/` (nếu M2 chưa tạo)
- Thêm `DbSet<TutorCertificate>` vào `ApplicationDbContext`
- Thêm actions `Certificates` (GET), `UploadCertificate` (POST) vào `AccountController`
- Tạo `Views/Account/Certificates.cshtml`
- Lưu file vào `wwwroot/uploads/certificates/`

**Out of Scope:**
- Không làm logic AI kiểm duyệt (`AiVerifyNote`) — đó là Module 12
- Không làm Admin duyệt chứng chỉ — đó là Module 8
- Không sửa TutorProfile — đó là Module 2

**Output cần có:**
- `Models/TutorCertificate.cs` (tạo mới nếu chưa có)
- `Controllers/AccountController.cs` — thêm `Certificates` GET, `UploadCertificate` POST
- `Views/Account/Certificates.cshtml`
- Migration mới nếu tạo model mới

**Business Rules cần implement:**
- Định dạng: JPG, PNG, PDF
- Kích thước tối đa: 10MB mỗi file
- Trạng thái mặc định: `IsVerified = false`
- Chỉ Tutor mới truy cập được (dùng `[Authorize(Roles = "Tutor")]`)

**Acceptance Criteria:**
- [ ] Upload thành công → hiển thị trong danh sách với trạng thái "Chờ xác minh"
- [ ] File sai định dạng → lỗi
- [ ] File > 10MB → lỗi
- [ ] Student truy cập trang này → 403 Forbidden
- [ ] Build pass

**Test Plan:**
1. `dotnet build` → pass
2. Login bằng tài khoản Tutor → truy cập `/Account/Certificates`
3. Upload file PDF hợp lệ → hiển thị trong danh sách, status "Chờ xác minh"
4. Upload file .exe → lỗi
5. Upload file > 10MB → lỗi
6. Login bằng tài khoản Student → truy cập URL trên → 403

**Ước tính:** 45 phút

---

## Tổng kết

| Task | Ước tính | Phụ thuộc |
|---|---|---|
| M1-T1 Email Service | 45 phút | — |
| M1-T2 OTP Service | 45 phút | T1 |
| M1-T3 Register | 60 phút | T2 |
| M1-T4 VerifyEmail | 60 phút | T3 |
| M1-T5 Login + Logout | 45 phút | T2 |
| M1-T6 Forgot + Reset Password | 90 phút | T4, T5 |
| M1-T7 Profile + Avatar | 60 phút | T5 |
| M1-T8 Change Password | 30 phút | T7 |
| M1-T9 Tutor Certificates | 45 phút | T7 |
| **Tổng** | **480 phút (~8 giờ)** | |

> Thư mục break tasks: `Docs/BreakTasks/`  
> File break task: `Docs/BreakTasks/BreakTask_M1_Auth.md`  
> Thư mục prompt: `Docs/Prompts/`  
> Tên file prompt: `M1_T1_EmailService.xml`, `M1_T2_OtpService.xml`, ...  
> Thư mục report: `Docs/Reports/`  
> Tên file report: `Report_M1_T1_[YYYY-MM-DD].md`, ...
