# FRS — StudyMate Platform
## Functional Requirement Specification (Tổng Quan)

> **Phiên bản:** 1.0  
> **Ngày:** 2026-07-10  
> **Dự án:** StudyMate — Nền tảng kết nối Gia sư & Học viên tích hợp AI  
> **Tech Stack:** ASP.NET Core MVC · EF Core · ASP.NET Identity · SQL Server (dev) · PostgreSQL (deploy) · SignalR · Python FastAPI (AI)

---

## 1. Tổng Quan Hệ Thống

StudyMate là nền tảng marketplace kết nối **Gia sư (Tutor)** và **Học viên (Student)**, tích hợp AI hỗ trợ tìm kiếm, ghép nối, và hỗ trợ học tập.

### 1.1 Actors

| Actor | Mô tả |
|---|---|
| **Guest** | Người dùng chưa đăng nhập, chỉ xem được trang public |
| **Student** | Học viên đã đăng ký, có thể đăng job, tìm và thuê gia sư |
| **Tutor** | Gia sư đã đăng ký, có hồ sơ, có thể apply job của học viên |
| **Admin** | Quản trị viên hệ thống, quản lý người dùng, nội dung, báo cáo |

### 1.2 Danh Sách Module

| Module | Tên | Nhóm phụ trách |
|---|---|---|
| M1 | Quản lý tài khoản & Xác thực | Nhóm 1 |
| M2 | Quản lý hồ sơ Gia sư | Nhóm 2 |
| M3 | Quản lý yêu cầu thuê Gia sư | Nhóm 3 |
| M4 | AI Matching | Nhóm 4 |
| M5 | Quy trình thuê (Application) | Nhóm 5 |
| M6 | Chat Realtime | Nhóm 6 |
| M7 | Lịch học | Nhóm 7 |
| M8 | Quản trị hệ thống | Nhóm 8 |
| M9 | Thanh toán | Bổ sung |
| M10 | Đánh giá | Bổ sung |
| M11 | Thông báo | Bổ sung |
| M12 | AI Learning Assistant | Bổ sung |

### 1.3 Dependency Giữa Các Module

```
M1 (Auth)
  └──▶ M2 (Tutor Profile)
  └──▶ M3 (Job Posting)
         └──▶ M4 (AI Matching)
         └──▶ M5 (Application)
                └──▶ M6 (Chat)
                └──▶ M7 (Booking)
                       └──▶ M9 (Payment)
                       └──▶ M10 (Review)
M11 (Notification) ◀── tất cả module trên
M12 (AI Assistant) ◀── M2, M3, M4, M7
M8  (Admin)        ◀── tất cả module trên
```

### 1.4 Phạm Vi MVP (Minimum Viable Product)

**Bắt buộc hoàn thành:**
- M1, M2, M3, M5, M6, M7, M8

**Khuyến khích hoàn thành:**
- M4 (AI Matching), M9 (Thanh toán), M10 (Review), M11 (Notification)

**Điểm cộng:**
- M12 (AI Learning Assistant)

---

## 2. Quy Tắc Chung Toàn Hệ Thống

### 2.1 Phân quyền
- Mọi route cần xác thực phải dùng `[Authorize]`
- Phân quyền theo Role: `[Authorize(Roles = "Tutor")]`
- Guest chỉ truy cập được trang public (Home, Search, TutorProfile detail)

### 2.2 Validation
- Server-side validation bắt buộc cho mọi form
- Client-side validation dùng jQuery Unobtrusive Validation
- Không tin tưởng dữ liệu từ client

### 2.3 Responsive
- Giao diện phải hoạt động tốt trên desktop và mobile
- Dùng Bootstrap 5 Grid System

### 2.4 Xử lý lỗi
- Trang 404 và 500 tùy chỉnh
- Log lỗi phía server
- Không hiển thị stack trace ra ngoài production

### 2.5 Ngôn ngữ
- Giao diện: Tiếng Việt
- Code, comment, biến: Tiếng Anh

---

## 3. File FRS Chi Tiết Theo Module

- [FRS_M1_Auth.md](./FRS_M1_Auth.md) — Quản lý tài khoản & Xác thực
- [FRS_M2_TutorProfile.md](./FRS_M2_TutorProfile.md) — Hồ sơ Gia sư
- [FRS_M3_JobPosting.md](./FRS_M3_JobPosting.md) — Yêu cầu thuê Gia sư
- [FRS_M4_AIMatching.md](./FRS_M4_AIMatching.md) — AI Matching
- [FRS_M5_Application.md](./FRS_M5_Application.md) — Quy trình thuê
- [FRS_M6_Chat.md](./FRS_M6_Chat.md) — Chat Realtime
- [FRS_M7_Booking.md](./FRS_M7_Booking.md) — Lịch học
- [FRS_M8_Admin.md](./FRS_M8_Admin.md) — Quản trị hệ thống
- [FRS_M9_Payment.md](./FRS_M9_Payment.md) — Thanh toán
- [FRS_M10_Review.md](./FRS_M10_Review.md) — Đánh giá
- [FRS_M11_Notification.md](./FRS_M11_Notification.md) — Thông báo
- [FRS_M12_AIAssistant.md](./FRS_M12_AIAssistant.md) — AI Learning Assistant
