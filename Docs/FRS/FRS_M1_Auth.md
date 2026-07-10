# FRS — Module 1: Quản Lý Tài Khoản & Xác Thực

> **Module:** M1  
> **Phiên bản:** 1.0  
> **Phụ thuộc:** Không có (module nền tảng)  
> **Bảng DB liên quan:** `AspNetUsers`, `AspNetRoles`, `AspNetUserRoles`, `OtpCodes`

---

## 1. Mô Tả Tổng Quan

Module quản lý toàn bộ vòng đời tài khoản người dùng: đăng ký, xác thực email bằng OTP, đăng nhập, quản lý hồ sơ cá nhân, đổi mật khẩu, quên mật khẩu và phân quyền theo Role.

---

## 2. Actors & Quyền Hạn

| Actor | Quyền |
|---|---|
| **Guest** | Đăng ký, Đăng nhập, Quên mật khẩu |
| **Student** | Xem/sửa hồ sơ cá nhân, Đổi mật khẩu, Upload avatar |
| **Tutor** | Xem/sửa hồ sơ cá nhân, Đổi mật khẩu, Upload avatar, Upload chứng chỉ |
| **Admin** | Xem/khóa/mở khóa tài khoản, Phân quyền |

---

## 3. Danh Sách Chức Năng

### UC-M1-01: Đăng Ký Tài Khoản

**Actor:** Guest  
**Mô tả:** Người dùng tạo tài khoản mới bằng email và mật khẩu.

**Luồng chính:**
1. Guest truy cập trang `/Account/Register`
2. Nhập: Họ tên, Email, Mật khẩu, Xác nhận mật khẩu, Chọn vai trò (Student/Tutor)
3. Hệ thống validate dữ liệu
4. Hệ thống tạo tài khoản với `IsEmailVerified = false`
5. Hệ thống tạo OTP 6 số, lưu vào `OtpCodes` với `Purpose = "EmailVerify"`, hạn 5 phút
6. Hệ thống gửi email chứa OTP
7. Chuyển hướng đến trang nhập OTP `/Account/VerifyEmail`

**Luồng thay thế:**
- Email đã tồn tại → hiển thị lỗi "Email này đã được sử dụng"
- Validation fail → hiển thị lỗi inline tại field tương ứng

**Business Rules:**
- Email phải hợp lệ và chưa tồn tại trong hệ thống
- Mật khẩu tối thiểu 8 ký tự, có ít nhất 1 chữ hoa, 1 số
- Xác nhận mật khẩu phải khớp
- Vai trò mặc định: Student nếu không chọn

**Acceptance Criteria:**
- [ ] Form hiển thị đúng các field
- [ ] Validation hiện lỗi inline, không reload trang
- [ ] Sau submit thành công → email OTP được gửi trong vòng 30 giây
- [ ] User được tạo trong DB với `EmailConfirmed = false`
- [ ] Redirect đến trang VerifyEmail

---

### UC-M1-02: Xác Thực Email bằng OTP

**Actor:** Guest (sau khi đăng ký)  
**Mô tả:** Xác thực email bằng mã OTP nhận qua email.

**Luồng chính:**
1. Guest nhập mã OTP 6 số tại `/Account/VerifyEmail`
2. Hệ thống kiểm tra OTP: đúng mã, chưa dùng, chưa hết hạn
3. Cập nhật `EmailConfirmed = true`, `IsEmailVerified = true`
4. Đánh dấu OTP `IsUsed = true`
5. Tự động đăng nhập và chuyển hướng về trang chủ

**Luồng thay thế:**
- OTP sai → hiển thị lỗi "Mã OTP không chính xác"
- OTP hết hạn → hiển thị lỗi "Mã OTP đã hết hạn" + nút "Gửi lại OTP"
- OTP đã dùng → hiển thị lỗi "Mã OTP đã được sử dụng"

**Business Rules:**
- OTP có hiệu lực 5 phút kể từ lúc tạo
- Mỗi lần gửi lại OTP tạo mã mới, OTP cũ bị vô hiệu
- Tối đa 3 lần nhập sai → khóa chức năng 15 phút

**Acceptance Criteria:**
- [ ] Nhập OTP đúng → tài khoản được xác thực, tự động login
- [ ] Nhập sai → hiển thị lỗi, không clear form
- [ ] Nút "Gửi lại OTP" hoạt động, OTP mới gửi trong 30 giây
- [ ] OTP hết hạn sau đúng 5 phút

---

### UC-M1-03: Đăng Nhập

**Actor:** Guest  
**Mô tả:** Người dùng đăng nhập bằng email và mật khẩu.

**Luồng chính:**
1. Guest truy cập `/Account/Login`
2. Nhập Email và Mật khẩu
3. Hệ thống xác thực thông tin
4. Tạo session/cookie đăng nhập
5. Chuyển hướng về trang trước đó hoặc trang chủ

**Luồng thay thế:**
- Email chưa xác thực → hiển thị lỗi "Vui lòng xác thực email trước khi đăng nhập" + nút "Gửi lại OTP"
- Sai email/mật khẩu → "Email hoặc mật khẩu không chính xác" (không nói rõ cái nào sai)
- Tài khoản bị khóa → "Tài khoản của bạn đã bị khóa. Liên hệ admin để được hỗ trợ."

**Business Rules:**
- Tài khoản chưa xác thực email không thể đăng nhập
- Tài khoản bị Admin khóa không thể đăng nhập
- Checkbox "Ghi nhớ đăng nhập" → cookie tồn tại 30 ngày
- Không checkbox → session cookie, hết hạn khi đóng trình duyệt

**Acceptance Criteria:**
- [ ] Đăng nhập đúng → redirect đúng trang
- [ ] Đăng nhập sai → hiển thị lỗi chung, không lộ thông tin
- [ ] "Ghi nhớ đăng nhập" hoạt động đúng
- [ ] Tài khoản chưa xác thực không login được

---

### UC-M1-04: Đăng Xuất

**Actor:** Student, Tutor, Admin  
**Mô tả:** Người dùng đăng xuất khỏi hệ thống.

**Luồng chính:**
1. User nhấn "Đăng xuất" trên navbar
2. Hệ thống xóa session/cookie
3. Chuyển hướng về trang chủ

**Acceptance Criteria:**
- [ ] Sau đăng xuất → không thể truy cập trang cần auth
- [ ] Cookie bị xóa hoàn toàn

---

### UC-M1-05: Quên Mật Khẩu

**Actor:** Guest  
**Mô tả:** Người dùng lấy lại mật khẩu qua email.

**Luồng chính:**
1. Guest truy cập `/Account/ForgotPassword`
2. Nhập email đã đăng ký
3. Hệ thống tạo OTP 6 số với `Purpose = "ForgotPassword"`, hạn 10 phút
4. Gửi email chứa OTP
5. Chuyển hướng đến trang xác nhận OTP `/Account/VerifyResetOtp`
6. Nhập OTP đúng → chuyển đến trang đặt mật khẩu mới `/Account/ResetPassword`
7. Nhập mật khẩu mới + xác nhận
8. Hệ thống cập nhật mật khẩu, vô hiệu tất cả OTP cũ
9. Đăng nhập tự động, chuyển về trang chủ

**Luồng thay thế:**
- Email không tồn tại → vẫn hiển thị "Nếu email tồn tại, chúng tôi sẽ gửi OTP" (bảo mật)

**Business Rules:**
- OTP reset password có hiệu lực 10 phút
- Sau reset thành công → tất cả session cũ bị invalidate

**Acceptance Criteria:**
- [ ] Flow đầy đủ 7 bước hoạt động đúng
- [ ] Không lộ thông tin tài khoản tồn tại hay không
- [ ] Mật khẩu cũ không dùng được sau khi reset

---

### UC-M1-06: Xem & Sửa Hồ Sơ Cá Nhân

**Actor:** Student, Tutor  
**Mô tả:** Người dùng xem và chỉnh sửa thông tin hồ sơ cá nhân.

**Thông tin hiển thị:**
- Avatar
- Họ tên
- Email (chỉ xem, không sửa)
- Số điện thoại
- Ngày sinh
- Giới tính
- Địa chỉ

**Acceptance Criteria:**
- [ ] Hiển thị đúng thông tin hiện tại
- [ ] Lưu thành công → hiển thị thông báo "Cập nhật thành công"
- [ ] Validation hoạt động

---

### UC-M1-07: Upload Avatar

**Actor:** Student, Tutor  
**Mô tả:** Người dùng thay ảnh đại diện.

**Business Rules:**
- Định dạng cho phép: JPG, PNG, WEBP
- Kích thước tối đa: 2MB
- Tự động resize về 300x300px
- Lưu vào thư mục `wwwroot/uploads/avatars/`

**Acceptance Criteria:**
- [ ] Upload thành công → avatar cập nhật ngay, không cần reload
- [ ] File quá lớn → hiển thị lỗi
- [ ] File sai định dạng → hiển thị lỗi

---

### UC-M1-08: Đổi Mật Khẩu

**Actor:** Student, Tutor  
**Mô tả:** Người dùng đã đăng nhập thay đổi mật khẩu.

**Luồng chính:**
1. Nhập mật khẩu hiện tại
2. Nhập mật khẩu mới + xác nhận
3. Hệ thống xác minh mật khẩu hiện tại
4. Cập nhật mật khẩu mới
5. Hiển thị thông báo thành công

**Business Rules:**
- Phải nhập đúng mật khẩu hiện tại mới được đổi
- Mật khẩu mới không được trùng mật khẩu hiện tại
- Mật khẩu mới phải đáp ứng độ phức tạp

**Acceptance Criteria:**
- [ ] Sai mật khẩu hiện tại → hiển thị lỗi
- [ ] Đổi thành công → thông báo thành công
- [ ] Mật khẩu cũ không còn dùng được

---

### UC-M1-09: Upload Chứng Chỉ (Tutor only)

**Actor:** Tutor  
**Mô tả:** Gia sư upload bằng cấp, chứng chỉ, IELTS để hệ thống kiểm duyệt.

**Business Rules:**
- Định dạng: JPG, PNG, PDF
- Kích thước tối đa: 10MB mỗi file
- Trạng thái sau upload: `IsVerified = false`, chờ AI/Admin kiểm duyệt
- AI kiểm duyệt ảnh → ghi kết quả vào `AiVerifyNote`

**Acceptance Criteria:**
- [ ] Upload thành công → hiển thị trong danh sách chứng chỉ với trạng thái "Chờ xác minh"
- [ ] File sai định dạng/quá lớn → hiển thị lỗi

---

### UC-M1-10: Phân Quyền (Admin only)

**Actor:** Admin  
**Mô tả:** Admin thay đổi role của người dùng.

**Acceptance Criteria:**
- [ ] Admin có thể thêm/xóa role của user
- [ ] Thay đổi có hiệu lực ngay lần đăng nhập tiếp theo

---

## 4. Out of Scope

- Đăng nhập bằng Google, Facebook, GitHub (OAuth)
- Xác thực 2 yếu tố (2FA)
- Bảo mật doanh nghiệp (SSO, LDAP)
- Quản lý session nhiều thiết bị

---

## 5. Bảng DB Liên Quan

| Bảng | Dùng cho |
|---|---|
| `AspNetUsers` | Lưu thông tin user |
| `AspNetRoles` | Lưu các role |
| `AspNetUserRoles` | Mapping user ↔ role |
| `OtpCodes` | Lưu mã OTP email verify & reset password |

---

## 6. Task Breakdown (30–90 phút)

| Task ID | Tên Task | Ước tính |
|---|---|---|
| M1-T1 | Setup EF Core + SQL Server + ApplicationDbContext | 30 phút |
| M1-T2 | Cấu hình ASP.NET Identity + ApplicationUser | 45 phút |
| M1-T3 | Seed Roles + Migration | 30 phút |
| M1-T4 | Trang Register + Validation | 60 phút |
| M1-T5 | Email Service (SMTP/SendGrid) | 45 phút |
| M1-T6 | OTP Service + Gửi OTP sau đăng ký | 60 phút |
| M1-T7 | Trang VerifyEmail + Logic xác thực OTP | 60 phút |
| M1-T8 | Trang Login + Remember Me | 45 phút |
| M1-T9 | Trang ForgotPassword + VerifyOTP + ResetPassword | 90 phút |
| M1-T10 | Trang Profile + Upload Avatar | 60 phút |
| M1-T11 | Trang ChangePassword | 30 phút |
| M1-T12 | Upload Chứng chỉ (Tutor) | 45 phút |
