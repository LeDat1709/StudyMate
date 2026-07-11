# FRS — Module 8: Quản Trị Hệ Thống (Admin)

> **Module:** M8  
> **Phiên bản:** 1.0  
> **Phụ thuộc:** Tất cả module  
> **Bảng DB:** `Reports`, `AiLogs` + truy vấn tất cả bảng

---

## 1. Mô Tả Tổng Quan

Module quản trị dành riêng cho Admin. Bao gồm dashboard thống kê tổng quan, quản lý người dùng, kiểm duyệt hồ sơ gia sư, xử lý báo cáo/khiếu nại và theo dõi log AI.

---

## 2. Actors & Quyền Hạn

| Actor | Quyền |
|---|---|
| **Admin** | Toàn quyền trên tất cả chức năng trong module này |

---

## 3. Danh Sách Chức Năng

### UC-M8-01: Dashboard Tổng Quan

**Mô tả:** Trang chính Admin hiển thị số liệu realtime.

**Các số liệu hiển thị:**

| Nhóm | Chỉ số |
|---|---|
| Users | Tổng user, user mới hôm nay, tổng Tutor, tổng Student |
| Jobs | Tổng job, job đang Open, job mới hôm nay |
| Bookings | Tổng booking, booking hôm nay, booking Completed |
| Revenue | Doanh thu hôm nay, tháng này, tổng |
| Rating | Rating trung bình toàn hệ thống |
| Reports | Báo cáo chờ xử lý |

**Giao diện:**
- Dạng card số liệu + biểu đồ đường (doanh thu theo ngày)
- Biểu đồ cột (user đăng ký theo tháng)
- Bảng top 5 gia sư rating cao nhất

**Acceptance Criteria:**
- [ ] Dashboard load trong 2 giây
- [ ] Số liệu chính xác với DB
- [ ] Biểu đồ hiển thị đúng

---

### UC-M8-02: Quản Lý Người Dùng

**Mô tả:** Admin xem, tìm kiếm, lọc và quản lý tài khoản.

**Chức năng:**
- Danh sách user với filter: Role / Trạng thái / Ngày đăng ký
- Xem chi tiết từng user
- Khóa/mở khóa tài khoản
- Đổi Role
- Xóa tài khoản (soft delete)

**Business Rules:**
- Không xóa cứng user có transaction
- Admin không tự khóa tài khoản Admin của mình

**Acceptance Criteria:**
- [ ] Danh sách user phân trang, tìm kiếm được
- [ ] Khóa user → user không đăng nhập được ngay lập tức
- [ ] Log hành động của Admin

---

### UC-M8-03: Kiểm Duyệt Hồ Sơ Gia Sư

**Mô tả:** Admin duyệt hồ sơ Tutor lần đầu trước khi hiển thị công khai.

**Chức năng:**
- Danh sách hồ sơ chờ duyệt
- Xem chi tiết hồ sơ + chứng chỉ
- Approve / Reject (kèm lý do)
- Xem ghi chú AI kiểm duyệt chứng chỉ

**Acceptance Criteria:**
- [ ] Approve → TutorProfile `IsVerified = true`, Tutor nhận notification
- [ ] Reject → Tutor nhận notification kèm lý do

---

### UC-M8-04: Quản Lý Job Posting

**Mô tả:** Admin xem, ẩn/hiện job vi phạm.

**Acceptance Criteria:**
- [ ] Ẩn job → không hiển thị công khai
- [ ] Admin thấy lý do ẩn trong log

---

### UC-M8-05: Xử Lý Báo Cáo / Khiếu Nại

**Mô tả:** Admin xem và xử lý các báo cáo từ người dùng.

**Giao diện:**
- Danh sách báo cáo, filter: Loại / Trạng thái / Ngày
- Chi tiết báo cáo: nội dung vi phạm, người bị báo cáo
- Hành động: Dismiss / Resolve (kèm ghi chú)

**Business Rules:**
- Giải quyết trong 48 giờ
- Nếu resolve → có thể kéo theo khóa tài khoản

**Acceptance Criteria:**
- [ ] Xử lý báo cáo thành công, `Status` cập nhật
- [ ] Người báo cáo nhận notification khi đã xử lý

---

### UC-M8-06: Quản Lý Đánh Giá

**Mô tả:** Admin xem và xóa đánh giá spam/vi phạm.

**Acceptance Criteria:**
- [ ] Admin xóa được review vi phạm
- [ ] AverageRating của Tutor tự cập nhật sau khi xóa review

---

### UC-M8-07: Xem AI Log

**Mô tả:** Admin theo dõi hoạt động của các AI service.

**Thông tin log:** Action, User, Input/Output, Model, Thời gian xử lý, Ngày

**Acceptance Criteria:**
- [ ] Danh sách log có thể filter theo Action và ngày
- [ ] Không hiển thị sensitive data trong log

---

## 4. Out of Scope

- Multi-admin role phân quyền chi tiết
- Audit log toàn bộ thao tác
- Export báo cáo PDF/Excel

---

## 5. Task Breakdown

| Task ID | Tên Task | Ước tính |
|---|---|---|
| M8-T1 | Layout Admin (sidebar, navbar) | 45 phút |
| M8-T2 | Dashboard + số liệu + biểu đồ | 90 phút |
| M8-T3 | Quản lý User (danh sách, khóa, đổi role) | 60 phút |
| M8-T4 | Kiểm duyệt hồ sơ Tutor | 60 phút |
| M8-T5 | Quản lý Job Posting | 45 phút |
| M8-T6 | Xử lý báo cáo / khiếu nại | 60 phút |
| M8-T7 | Quản lý Review | 30 phút |
| M8-T8 | Xem AI Log | 30 phút |
| M8-T9 | Phân quyền Admin middleware | 30 phút |
| M8-T10 | Quản lý Subjects (thêm/sửa/xóa môn học) | 30 phút |
