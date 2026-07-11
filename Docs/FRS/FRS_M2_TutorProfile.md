# FRS — Module 2: Quản Lý Hồ Sơ Gia Sư

> **Module:** M2  
> **Phiên bản:** 1.0  
> **Phụ thuộc:** M1 (Auth)  
> **Bảng DB:** `TutorProfiles`, `TutorSubjects`, `TutorCertificates`, `TutorAvailabilities`, `DemoLessons`, `Subjects`

---

## 1. Mô Tả Tổng Quan

Module cho phép Gia sư tạo và quản lý hồ sơ cá nhân theo phong cách LinkedIn — thể hiện năng lực, kinh nghiệm, chứng chỉ, lịch rảnh và video giới thiệu. Học viên và Guest có thể xem hồ sơ công khai. Admin và AI có thể kiểm duyệt chứng chỉ.

---

## 2. Actors & Quyền Hạn

| Actor | Quyền |
|---|---|
| **Guest** | Xem hồ sơ công khai của gia sư |
| **Student** | Xem hồ sơ gia sư, xem demo lesson |
| **Tutor** | CRUD hồ sơ của mình, upload chứng chỉ, quản lý lịch rảnh |
| **Admin** | Xem tất cả hồ sơ, duyệt/từ chối chứng chỉ, ẩn hồ sơ vi phạm |

---

## 3. Danh Sách Chức Năng

### UC-M2-01: Tạo Hồ Sơ Gia Sư

**Actor:** Tutor  
**Mô tả:** Gia sư tạo hồ sơ lần đầu sau khi đăng ký tài khoản.

**Thông tin hồ sơ:**

| Field | Bắt buộc | Mô tả |
|---|---|---|
| Headline | ✅ | Tiêu đề ngắn VD: "Gia sư IELTS 8.0+" |
| Bio | ✅ | Giới thiệu bản thân, tối thiểu 100 ký tự |
| Ảnh đại diện | ✅ | Kế thừa từ M1 |
| Video giới thiệu | ❌ | URL YouTube hoặc upload MP4 ≤ 50MB |
| Môn dạy | ✅ | Chọn từ danh sách có sẵn, tối thiểu 1 môn |
| Kinh nghiệm (năm) | ✅ | Số nguyên ≥ 0 |
| Trình độ học vấn | ✅ | Cử nhân / Thạc sĩ / Tiến sĩ / Khác |
| Học phí / giờ | ✅ | VNĐ, > 0 |
| Hình thức dạy | ✅ | Online / Offline / Cả hai |
| Địa chỉ | ❌ | Bắt buộc nếu dạy Offline |
| Trạng thái | ✅ | Đang nhận học viên / Tạm dừng |

**Luồng chính:**
1. Tutor vào `/Tutor/Profile/Create`
2. Điền thông tin hồ sơ theo từng section
3. Hệ thống validate
4. Lưu hồ sơ với trạng thái `IsVerified = false` (chờ admin duyệt lần đầu)
5. Hiển thị thông báo "Hồ sơ đang chờ xét duyệt"

**Business Rules:**
- Mỗi Tutor chỉ có 1 TutorProfile
- Hồ sơ phải được Admin duyệt lần đầu trước khi hiển thị công khai
- Dạy Offline bắt buộc nhập địa chỉ

**Acceptance Criteria:**
- [ ] Form đầy đủ các field, validation hoạt động
- [ ] Lưu thành công → bản ghi trong `TutorProfiles`
- [ ] Tutor nhìn thấy trạng thái "Chờ duyệt"

---

### UC-M2-02: Chỉnh Sửa Hồ Sơ

**Actor:** Tutor  
**Mô tả:** Gia sư cập nhật thông tin hồ sơ.

**Business Rules:**
- Chỉ Tutor chủ sở hữu mới sửa được hồ sơ của mình
- Sau khi sửa, `UpdatedAt` được cập nhật
- Thay đổi hiển thị công khai ngay (không cần duyệt lại trừ khi Admin cấu hình khác)

**Acceptance Criteria:**
- [ ] Sửa thành công → dữ liệu được cập nhật trong DB
- [ ] Tutor khác không thể sửa hồ sơ của người khác (trả về 403)

---

### UC-M2-03: Upload Ảnh Đại Diện

**Actor:** Tutor  
**Mô tả:** Tutor upload ảnh đại diện cho hồ sơ gia sư.

**Business Rules:**
- Định dạng: JPG, PNG, WEBP
- Kích thước: ≤ 2MB
- Tự động resize về 400x400px
- AI kiểm duyệt ảnh (không phù hợp → từ chối, ghi log)

**Acceptance Criteria:**
- [ ] Upload thành công → ảnh hiển thị ngay trên hồ sơ
- [ ] AI flag ảnh không phù hợp → hiển thị thông báo lỗi

---

### UC-M2-04: Upload Video Giới Thiệu

**Actor:** Tutor  
**Mô tả:** Tutor upload video tự giới thiệu để học viên xem trước.

**Business Rules:**
- Hỗ trợ: URL YouTube/Vimeo HOẶC upload file MP4 ≤ 50MB
- Chỉ 1 video tại một thời điểm
- Video mới thay thế video cũ

**Acceptance Criteria:**
- [ ] Video YouTube embed được trên trang hồ sơ
- [ ] File MP4 upload được, hiển thị player

---

### UC-M2-05: Quản Lý Môn Dạy

**Actor:** Tutor  
**Mô tả:** Gia sư chọn các môn học mình có thể dạy.

**Business Rules:**
- Chọn từ danh sách `Subjects` có sẵn
- Tối thiểu 1 môn, tối đa 10 môn
- Admin có thể thêm môn mới vào danh sách

**Acceptance Criteria:**
- [ ] Hiển thị danh sách môn dạng checkbox/tag
- [ ] Lưu/xóa môn dạy thành công

---

### UC-M2-06: Upload Chứng Chỉ / Bằng Cấp / IELTS

**Actor:** Tutor  
**Mô tả:** Gia sư upload các giấy tờ chứng minh năng lực.

**Thông tin mỗi chứng chỉ:**
- Tên chứng chỉ (VD: "IELTS 8.0")
- Loại: Bằng cấp / Chứng chỉ / IELTS / TOEIC
- Đơn vị cấp
- Ngày cấp
- File đính kèm (JPG, PNG, PDF ≤ 10MB)

**Business Rules:**
- Sau upload → `IsVerified = false`
- AI tự động kiểm duyệt ảnh, ghi vào `AiVerifyNote`
- Admin có thể duyệt/từ chối thủ công

**Acceptance Criteria:**
- [ ] Upload thành công → hiển thị trong danh sách với badge "Chờ xác minh"
- [ ] Sau khi duyệt → badge đổi thành "Đã xác minh" ✅
- [ ] AI ghi chú kiểm duyệt vào `AiVerifyNote`

---

### UC-M2-07: Quản Lý Lịch Rảnh

**Actor:** Tutor  
**Mô tả:** Gia sư cài đặt khung giờ có thể dạy trong tuần.

**Giao diện:** Bảng 7 cột (Thứ 2 → Chủ nhật), mỗi ô là 1 khung giờ.

**Business Rules:**
- Mỗi entry: Thứ trong tuần + Giờ bắt đầu + Giờ kết thúc
- Có thể thêm nhiều khung giờ cùng ngày
- Giờ bắt đầu phải trước giờ kết thúc

**Acceptance Criteria:**
- [ ] Lưu/xóa khung giờ thành công
- [ ] Hiển thị lịch rảnh đẹp trên trang hồ sơ công khai

---

### UC-M2-08: Quản Lý Demo Lesson

**Actor:** Tutor  
**Mô tả:** Gia sư đăng các bài học mẫu để học viên xem thử.

**Thông tin demo lesson:**
- Tiêu đề
- Mô tả
- Video URL hoặc upload

**Acceptance Criteria:**
- [ ] Thêm/sửa/xóa demo lesson thành công
- [ ] Hiển thị đúng trên trang hồ sơ công khai

---

### UC-M2-09: Xem Hồ Sơ Công Khai

**Actor:** Guest, Student, Tutor  
**Mô tả:** Xem hồ sơ đầy đủ của một gia sư.

**Trang hồ sơ công khai hiển thị:**
- Ảnh đại diện + Headline + Rating
- Bio
- Môn dạy
- Kinh nghiệm + Trình độ
- Học phí
- Chứng chỉ đã được xác minh
- Lịch rảnh
- Demo lessons
- Đánh giá từ học viên (module M10)
- Nút "Liên hệ / Thuê gia sư" (chỉ hiện với Student đã đăng nhập)

**Business Rules:**
- Chỉ hiển thị hồ sơ đã được Admin duyệt (`IsVerified = true`)
- Hoặc Admin có thể cấu hình auto-approve

**Acceptance Criteria:**
- [ ] Trang hiển thị đầy đủ thông tin
- [ ] Guest xem được nhưng không thấy nút liên hệ
- [ ] Responsive trên mobile

---

### UC-M2-10: Tìm Kiếm & Lọc Gia Sư

**Actor:** Guest, Student  
**Mô tả:** Tìm kiếm gia sư theo môn học, học phí, hình thức dạy, rating.

**Bộ lọc:**
- Môn học
- Học phí (khoảng min-max)
- Hình thức: Online / Offline / Cả hai
- Trình độ gia sư
- Rating tối thiểu
- Địa điểm (nếu Offline)

**Sắp xếp:** Rating cao nhất / Học phí thấp nhất / Mới nhất

**Acceptance Criteria:**
- [ ] Tìm kiếm trả về kết quả đúng với bộ lọc
- [ ] Phân trang, mỗi trang 12 kết quả
- [ ] URL có query string để share được

---

## 4. Out of Scope

- Livestream dạy học
- Ký hợp đồng điện tử
- Tích hợp portfolio bên ngoài

---

## 5. Task Breakdown

| Task ID | Tên Task | Ước tính |
|---|---|---|
| M2-T1 | Tạo TutorProfile + TutorSubjects Models & Migration | 45 phút |
| M2-T2 | Form tạo/sửa hồ sơ gia sư (section cơ bản) | 90 phút |
| M2-T3 | Upload ảnh đại diện + Video giới thiệu | 60 phút |
| M2-T4 | Quản lý môn dạy (multi-select) | 45 phút |
| M2-T5 | Upload & quản lý chứng chỉ | 60 phút |
| M2-T6 | Quản lý lịch rảnh (weekly schedule UI) | 90 phút |
| M2-T7 | Trang hồ sơ công khai | 60 phút |
| M2-T8 | Trang tìm kiếm & lọc gia sư | 90 phút |
| M2-T9 | Demo Lesson CRUD | 45 phút |
| M2-T10 | AI kiểm duyệt ảnh/chứng chỉ (gọi Python API) | 60 phút |
