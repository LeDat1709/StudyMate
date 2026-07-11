# FRS — Module 11: Thông Báo (Notification)

> **Module:** M11  
> **Phiên bản:** 1.0  
> **Phụ thuộc:** Tất cả module  
> **Bảng DB:** `Notifications`  
> **Tech:** SignalR (realtime) + Background Service (delayed)

---

## 1. Mô Tả Tổng Quan

Module thông báo realtime và định kỳ, kết hợp SignalR để push thông báo in-app ngay lập tức và Background Service để gửi reminder theo lịch.

---

## 2. Danh Sách Trigger Thông Báo

| Sự kiện | Người nhận | Nội dung |
|---|---|---|
| Job mới được đăng | Tutor phù hợp (AI gợi ý) | "Có job mới phù hợp với bạn: [Tên job]" |
| Tutor apply job | Student | "Gia sư [Tên] đã apply job [Tên job] của bạn" |
| Accept application | Tutor | "Chúc mừng! Bạn đã được chấp nhận bởi [Tên Student]" |
| Reject application | Tutor | "Yêu cầu của bạn vào job [Tên] chưa được chấp nhận" |
| Booking mới | Tutor | "Học viên [Tên] đặt lịch học ngày [Ngày]" |
| Booking confirmed | Student | "Gia sư [Tên] đã xác nhận lịch học của bạn" |
| Reminder 24h | Student + Tutor | "Nhắc nhở: Buổi học của bạn vào [Ngày] lúc [Giờ]" |
| Reminder 1h | Student + Tutor | "Buổi học sắp bắt đầu sau 1 giờ" |
| Booking completed | Student | "Buổi học hoàn thành. Hãy đánh giá gia sư nhé!" |
| Tin nhắn mới | Người nhận | "Bạn có tin nhắn mới từ [Tên]" |
| Thanh toán | Student + Tutor | "Giao dịch [Loại] [Số tiền] thành công" |
| Đánh giá mới | Tutor | "Bạn có đánh giá mới từ học viên [Tên]" |

---

## 3. Danh Sách Chức Năng

### UC-M11-01: Hiển Thị Thông Báo Realtime

**Mô tả:** Icon chuông trên navbar, badge số chưa đọc, dropdown xem nhanh.

**Acceptance Criteria:**
- [ ] Badge cập nhật realtime qua SignalR
- [ ] Dropdown hiển thị 5 thông báo mới nhất
- [ ] Click vào thông báo → navigate đến trang liên quan

---

### UC-M11-02: Trang Tất Cả Thông Báo

**Mô tả:** `/Notifications` — xem toàn bộ thông báo với phân trang.

**Filter:** Tất cả / Chưa đọc / Theo loại

**Acceptance Criteria:**
- [ ] Hiển thị đầy đủ, phân trang
- [ ] Đánh dấu tất cả đã đọc

---

### UC-M11-03: Đánh Dấu Đã Đọc

**Mô tả:** Click vào thông báo → `IsRead = true`.

**Acceptance Criteria:**
- [ ] Đánh dấu đơn lẻ và đánh dấu tất cả hoạt động
- [ ] Badge số giảm tương ứng

---

## 4. Out of Scope

- Push notification mobile (FCM/APNs)
- Email digest
- Notification preferences tùy chỉnh

---

## 5. Task Breakdown

| Task ID | Tên Task | Ước tính |
|---|---|---|
| M11-T1 | Notification Model + Migration | 30 phút |
| M11-T2 | NotificationService (tạo + gửi realtime qua SignalR) | 60 phút |
| M11-T3 | Navbar badge + dropdown thông báo | 60 phút |
| M11-T4 | Trang tất cả thông báo | 45 phút |
| M11-T5 | Background job gửi reminder | 45 phút |
